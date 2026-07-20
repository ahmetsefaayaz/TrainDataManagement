const map = L.map('map').setView([39.9351, 32.8435], 6);

L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    maxZoom: 19,
    attribution: '© OpenStreetMap'
}).addTo(map);

let currentPolyline = null;
let currentPatchedPolyline = null;
let currentMarkers = [];

async function fetchRoute() {
    const locoId = document.getElementById('locoId').value;
    const startDate = document.getElementById('startDate').value;
    const endDate = document.getElementById('endDate').value;
    const btn = document.getElementById('drawBtn');

    if (!locoId) {
        alert("Lütfen geçerli bir Lokomotif ID girin! (Örn: 1)");
        return;
    }

    if (!startDate || !endDate) {
        alert("Lütfen başlangıç ve bitiş tarihlerini seçin.");
        return;
    }

    const url = `http://localhost:5215/api/telemetry?startDate=${startDate}&endDate=${endDate}&locomotiveId=${locoId}`;

    try {
        btn.textContent = "Veriler Çekiliyor...";
        btn.disabled = true;

        const response = await fetch(url);

        if (!response.ok) {
            throw new Error("HTTP Hatası: " + response.status);
        }

        const data = await response.json();

        if (data.length === 0) {
            alert("Bu tarih aralığında, bu lokomotife ait kayıt bulunamadı.");
            return;
        }

        const latLngs = data.map(point => [point.latitude, point.longitude]);

        if (currentPolyline) {
            map.removeLayer(currentPolyline);
        }
        if (currentPatchedPolyline) {
            map.removeLayer(currentPatchedPolyline);
        }
        currentMarkers.forEach(marker => map.removeLayer(marker));
        currentMarkers = [];
        
        const segments = [];
        const patchedSegments = [];
        let currentSegment = [];
        
        //Şimdilik manuel olarak 4 yazdık
        const currentRouteId = 4;

        for (let i = 0; i < data.length; i++) {
            const point = data[i];
            const currentPointTime = new Date(point.recordedAt || point.RecordedAt).getTime();

            if (i > 0) {
                const prevPoint = data[i - 1];
                const prevPointTime = new Date(prevPoint.recordedAt || prevPoint.RecordedAt).getTime();

                const timeDiff = currentPointTime - prevPointTime;

                if (timeDiff > 200) {
                    segments.push(currentSegment);
                    currentSegment = [];

                    try {
                        const fixUrl = `http://localhost:5215/api/fixdata/fix?startLat=${prevPoint.latitude}&startLon=${prevPoint.longitude}&endLat=${point.latitude}&endLon=${point.longitude}&routeId=${currentRouteId}`;
                        
                        const fixResponse = await fetch(fixUrl);

                        if(fixResponse.ok) {
                            const missingWaypoints = await fixResponse.json();
                            const patchedCoords = missingWaypoints.map(w => [w.latitude, w.longitude]);
                            if(patchedCoords.length > 1) {
                                patchedSegments.push(patchedCoords);
                            }
                        }
                    } catch(err) {
                        console.error("Yama verisi çekilemedi: ", err);
                    }
                    
                }
            }

            currentSegment.push([point.latitude, point.longitude]);

            if (i === data.length - 1) {
                segments.push(currentSegment);
            }
        }
        const validSegments = segments.filter(seg => seg.length > 1);
        
        if (validSegments.length > 0) {
            currentPolyline = L.polyline(validSegments, { color: 'blue', weight: 4 }).addTo(map);
            map.fitBounds(currentPolyline.getBounds());
        }

        if (patchedSegments.length > 0) {
            currentPatchedPolyline = L.polyline(patchedSegments, {
                color: 'gray',
                weight: 4,
                dashArray: '10, 10'
            }).addTo(map);
        }
        
        //Hız 0 olduysa kırmızı nokta oluştur.
        const knownStops = [];
        data.forEach(point => {
            if (point.speed === 0 || point.Speed === 0) {

                const alreadyAdded = knownStops.some(
                    stop => Math.abs(stop.lat - point.latitude) < 0.0001 && Math.abs(stop.lon - point.longitude) < 0.0001
                );

                if (!alreadyAdded) {
                    knownStops.push({ lat: point.latitude, lon: point.longitude });

                    const marker = L.circleMarker([point.latitude, point.longitude], {
                        color: 'darkred',
                        fillColor: 'red',
                        fillOpacity: 1,
                        radius: 6
                    }).addTo(map);

                    const timeString = new Date(point.recordedAt || point.RecordedAt).toLocaleTimeString('tr-TR');
                    marker.bindPopup(`<b>İstasyon / Bekleme Noktası</b><br>Varış: ${timeString}`);

                    currentMarkers.push(marker);
                }
            }
        });

        map.fitBounds(currentPolyline.getBounds());
        console.log(`Başarılı! Lokomotif ${locoId} için rota çizildi.`);

    } catch (error) {
        console.error("Hata:", error);
        alert("Veri çekilemedi. Konsolu kontrol edin.");
    } finally {
        btn.textContent = "Rotayı Çiz";
        btn.disabled = false;
    }
}

document.addEventListener("DOMContentLoaded", () => {
    const now = new Date();
    const oneHourAgo = new Date(now.getTime() - (60 * 60 * 1000));
    document.getElementById('startDate').value = oneHourAgo.toISOString().slice(0, 16);
    document.getElementById('endDate').value = now.toISOString().slice(0, 16);
});