"use strict";
(function () {
    console.log("Injected...");

    // Function to process links when the target div is found
    function processLinks() {
        const baseContentDiv = window.document.querySelector('div[rel="base_content"]');
        if (!baseContentDiv) {
            // console.log("Target div not found");
            return;
        }

        let hrefs = [];
        let links = baseContentDiv.querySelectorAll('ul > li > a');

        links.forEach(link => {
            let href = link.getAttribute('href');
            if (href && href.indexOf("http") === 0) {
                hrefs.push(href);
            }
        });

        console.log("Found links:", hrefs);
        
        if (hrefs.length > 0) {
            console.log("Sending links to background script");
            chrome.runtime.sendMessage({
                type: "links",
                links: hrefs,
                pageUrl: document.location.href
            });
        }
    }

    processLinks();
})();