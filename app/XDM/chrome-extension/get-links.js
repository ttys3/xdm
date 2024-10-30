"use strict";
(function () {
    console.log("Injected...");

    // batch download 115 files
    // Select the div with the attribute rel="base_content"
    let baseContentDiv = document.querySelector('div[rel="base_content"]');

    // Check if the div was found
    if (baseContentDiv) {
        // Select all a elements that are descendants of li elements within ul under the baseContentDiv
        let links = baseContentDiv.querySelectorAll('ul > li > a');

        // Create an array to store the href values
        let hrefs = [];

        // Iterate over the NodeList and push each href value into the array
        links.forEach(link => {
            let href = link.getAttribute('href');
            if (href && href.indexOf("http") === 0) {
                hrefs.push(href);
            }
        });

        // Optional: Log the array of href values to verify the result
        console.log(hrefs);
    } else {
        console.error('The div with rel="base_content" was not found.');
    }

    console.log(hrefs);
    chrome.runtime.sendMessage({
        type: "links",
        links: hrefs,
        pageUrl: document.location.href
    });
})();