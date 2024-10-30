"use strict";
(function () {
    console.log("Injected...");

    // Function to process links when the target div is found
    function processLinks(baseContentDiv) {
        let hrefs = [];
        let links = baseContentDiv.querySelectorAll('ul > li > a');

        links.forEach(link => {
            let href = link.getAttribute('href');
            if (href && href.indexOf("http") === 0) {
                hrefs.push(href);
            }
        });

        console.log("Found links:", hrefs);
        
        chrome.runtime.sendMessage({
            type: "links",
            links: hrefs,
            pageUrl: document.location.href
        });
    }

    // Create a MutationObserver to watch for the div
    const observer = new MutationObserver((mutations, obs) => {
        const baseContentDiv = document.querySelector('div[rel="base_content"]');
        if (baseContentDiv) {
            console.log("Target div found");
            obs.disconnect(); // Stop observing once we find the element
            processLinks(baseContentDiv);
        }
    });

    // Start observing the document with the configured parameters
    observer.observe(document.body, {
        childList: true,
        subtree: true
    });

    // Set a timeout to stop the observer after 30 seconds to prevent infinite observation
    setTimeout(() => {
        observer.disconnect();
        console.log("Observer timed out after 30 seconds");
    }, 30000);
})();