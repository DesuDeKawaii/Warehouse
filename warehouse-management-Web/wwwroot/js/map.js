window.addMarker = function (latitude, longitude, name) {
    ymaps.ready(function () {
        // Инициализация карты, если она еще не была создана
        if (!window.myMap) {
            window.myMap = new ymaps.Map('map', {
                center: [latitude, longitude],
                zoom: 10
            });
        }

        // Добавление маркера на карту с описанием
        var myPlacemark = new ymaps.Placemark([latitude, longitude], {
            balloonContent: name // Показать имя склада в баллоне
        });

        window.myMap.geoObjects.add(myPlacemark);
    });
};
