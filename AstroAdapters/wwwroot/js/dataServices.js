const globals = {
    cacheName: "astroAdapt",
    manufacturers: "/data/manufacturers.json",
    preferences: "/data/preferences.json"
};

const retrieve = async function (path) {
    const cache = await caches.open(globals.cacheName);
    const res = await cache.match(path);
    if (res) {
        const jsonStr = await res.text();
        return jsonStr;
    }
    const response = await fetch(path);
    if (response) {
        const responseStr = await response.clone().text();
        cache.put(path, response);
        return responseStr;
    }
    return '';
}

const save = async function (path, payload) {
    const cache = await caches.open(globals.cacheName);
    const blob = new Blob([payload], {
        type: "application/json",
        status: 200,
        statusText: `Stored at ${new Date()}`
    });
    const headers = new Headers({
        "content-length": blob.size
    });
    const response = new Response(blob, headers);
    cache.put(path, response);
    return true;
}

const getManufacturers = async () => await retrieve(globals.manufacturers);
const getPreferences = async () => await retrieve(globals.preferences);
const putPreferences = async (payload) => await save(globals.preferences, payload);

export { getManufacturers, getPreferences, putPreferences };