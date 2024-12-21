// loadScript: returns a promise that completes when the script loads
window.loadScript = (function () {
    const loaded = {};

    return function (scriptPath) {
        // check list - if already loaded we can ignore
        if (loaded[scriptPath]) {
            // return 'empty' promise
            return Promise.resolve();
        }

        return new Promise((resolve, reject) => {
            // create JS library script element
            const script = document.createElement("script");
            script.src = scriptPath;
            script.type = "text/javascript";

            // flag as loading/loaded
            loaded[scriptPath] = true;

            // if the script returns okay, return resolve
            script.onload = () => resolve(scriptPath);

            // if it fails, return reject
            script.onerror = () => {
                console.error(`${scriptPath} load failed`);
                reject(scriptPath);
            };

            // scripts will load at end of body
            document.body.appendChild(script);
        });
    };
})();
