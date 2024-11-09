﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using TraceLog;
using XDM.Core;
using XDM.Core.Util;

namespace XDM.Core.Clients.Http
{
    public static class WebRequestExtensions
    {
        private static readonly char[] QuoteChars = new char[] { '"' };
        public static void EnsureSuccessStatusCode(this HttpWebResponse response)
        {
            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.PartialContent)
            {
                throw new HttpException(response.StatusDescription, null, response.StatusCode);
            }
        }

        public static void EnsureSuccessStatusCode(HttpStatusCode statusCode, string? statusDescription)
        {
            if (statusCode != HttpStatusCode.OK && statusCode != HttpStatusCode.PartialContent)
            {
                throw new HttpException(statusDescription ?? "Invalid response", null, statusCode);
            }
        }

        public static void Discard(this HttpWebResponse response)
        {
#if NET35
            var bytes = new byte[8192];
#else
            var bytes = System.Buffers.ArrayPool<byte>.Shared.Rent(8192);
#endif
            try
            {
                using var stream = response.GetResponseStream();
                while (true)
                {
                    int x = stream.Read(bytes, 0, bytes.Length);
                    if (x == 0) break;
                }
            }
            finally
            {
#if !NET35
                System.Buffers.ArrayPool<byte>.Shared.Return(bytes);
#endif
            }
        }

        public static long GetContentLength(this HttpWebResponse response)
        {
            try
            {
                if (response.ContentLength > 0) return response.ContentLength;
                if (response.ContentEncoding != null &&
                    response.ContentEncoding != "none" && response.ContentEncoding != "identity")
                {
                    return -1;
                }
                var contentRange = response.Headers.GetValues("Content-Range")?.First();
                return ContentLengthFromContentRange(contentRange);
            }
            catch { }
            return -1;
        }

        public static long ContentLengthFromContentRange(string? contentRange)
        {
            try
            {
                if (contentRange == null) return -1;
                var index = contentRange.IndexOf('/');
                if (index == -1) return -1;
                if (Int64.TryParse(contentRange.Substring(index + 1).Trim(), out long result))
                {
                    return result;
                }
            }
            catch { }
            return -1;
        }

        public static string? GetContentDispositionFileName(this HttpWebResponse response)
        {
            return GetContentDispositionFileName(response.Headers.Get("Content-Disposition"));
        }

        public static string? GetContentDispositionFileName(string contentDisposition)
        {
            //var contentDisposition = response.Headers.Get("Content-Disposition");
            if (string.IsNullOrEmpty(contentDisposition))
            {
                return null;
            }

            Log.Debug("Trying to get filename from: " + contentDisposition);
            try
            {
                var headerValue = System.Net.Http.Headers.ContentDispositionHeaderValue.Parse(contentDisposition);

                if (headerValue.FileNameStar != null)
                {
                    return FileHelper.SanitizeFileName(Uri.UnescapeDataString(headerValue.FileNameStar));
                }
                else if (headerValue.FileName != null)
                {
                    // https://github.com/dotnet/runtime/issues/32765#issuecomment-766768351
                    // response.Content.Headers.ContentDisposition.FileName contains quotes #32765
                    var fileName = headerValue.FileName.Trim(QuoteChars);
                    // Try to decode filename if it contains non-ASCII characters
                    // for example:
                    // content-disposition: attachment; filename="八月.zip"
                    // in dotnet we will get fileName like: `attachment; filename="åxxx«æxxx.zip"`
                    try 
                    {
                        byte[] bytes = Encoding.GetEncoding("ISO-8859-1").GetBytes(fileName);
                        fileName = Encoding.UTF8.GetString(bytes);
                    }
                    catch (Exception ex)
                    {
                        Log.Debug($"Failed to decode filename: {ex.Message}");
                        // Keep original filename if decode fails
                    }

                    return FileHelper.SanitizeFileName(fileName);
                }
            }
            catch (FormatException ex)
            {
                // Invalid Content-Disposition header format
                Log.Debug(ex, ex.Message);
            }

            return null;
        }

        public static string ReadAsString(this HttpWebResponse response, CancelFlag cancellationToken)
        {
#if NET35
            var buf = new byte[8192];
#else
            var buf = System.Buffers.ArrayPool<byte>.Shared.Rent(8192);
#endif
            try
            {
                var sourceStream = response.GetResponseStream();
                var ms = new MemoryStream();
                while (!cancellationToken.IsCancellationRequested)
                {
                    var x = sourceStream.Read(buf, 0, buf.Length);
                    if (x == 0)
                    {
                        break;
                    }
                    ms.Write(buf, 0, x);
                    cancellationToken.ThrowIfCancellationRequested();
                }
                return Encoding.UTF8.GetString(ms.ToArray());
            }
            finally
            {
#if !NET35
                System.Buffers.ArrayPool<byte>.Shared.Return(buf);
#endif
            }

        }

#if NET35

        public static void AddRange(this HttpWebRequest request, long start, long end)
        {
            var method = typeof(WebHeaderCollection).GetMethod
                        ("AddWithoutValidate", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            string key = "Range";
            string val = string.Format("bytes={0}-{1}", start, end);

            method.Invoke(request.Headers, new object[] { key, val });
        }

        public static void AddRange(this HttpWebRequest request, long start)
        {
            var method = typeof(WebHeaderCollection).GetMethod
                        ("AddWithoutValidate", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            string key = "Range";
            string val = string.Format("bytes={0}-", start);

            method.Invoke(request.Headers, new object[] { key, val });
        }
#endif
    }
}
