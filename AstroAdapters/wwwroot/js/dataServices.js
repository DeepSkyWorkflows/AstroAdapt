const globals = {
    cacheName: "astroAdapt",
    manufacturers: "/data/manufacturers.json",
    preferences: "/data/preferences.json",
    defaultImages: "/images",
    extensions: "/imageExtensions",
    images: "/data/imageCache"
};

const resolveImage = async function (id, componentType) {
    const extensionsPath = `${globals.extensions}/${id}`;
    const cachedPath = `${globals.images}/${id}`;
    const fetchPaths = [
        `${globals.defaultImages}/${id}.png`,
        `${globals.defaultImages}/${id}.jpg`,
        `${globals.defaultImages}/${id}.jpeg`,
        `${globals.defaultImages}/${id}.tif`,
        `${globals.defaultImages}/${id}.tiff`,
        `${globals.defaultImages}/${id}.gif`,
        `${globals.defaultImages}/${componentType}.png`];
    const cache = await caches.open(globals.cacheName);
    const res = await cache.match(extensionsPath);
    let extension = 'png';
    if (res) {
        extension = await res.text();
        const imagePath = `${cachedPath}.${extension}`;
        const responseImage = await cache.match(imagePath);
        if (responseImage) {
            const imageBlob = await responseImage.blob();
            return URL.createObjectURL(imageBlob);            
        }
    }
    for (let idx = 0; idx < fetchPaths.length; idx++) {

        const response = await fetch(fetchPaths[idx]);

        if (response && response.ok) {

            const responseBlob = await response.clone().blob();

            cache.put(extensionsPath, new Response(fetchPaths[idx].split('.')[1]));
            cache.put(`${cachedPath}.${extension}`, response);

            return URL.createObjectURL(responseBlob);
        }
    }
}

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

export { getManufacturers, getPreferences, putPreferences, resolveImage };