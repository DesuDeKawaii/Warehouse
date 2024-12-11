var map;

function initializeMap(mapsKey) {
    atlas.setSubscriptionKey(mapsKey);
    map = new atlas.Map("mapDiv", {
        view: "Auto",
        center: [-122.129, 47.640],
        zoom: 3
    });
}

function addMarker(position) {
    const marker = new atlas.HtmlMarker(position.latitude, position.longitude);

    map.markers.add(marker);
    return marker;
}

function requestUserLocation() {
    return new Promise((resolve, reject) => {
        if (navigator.geolocation) {
            navigator.geolocation.getCurrentPosition(
                position => resolve({ latitude: position.coords.latitude, longitude: position.coords.longitude }),
                error => reject(error)
            );
        } else {
            reject("Geolocation is not supported by this browser.");
        }
    });
}