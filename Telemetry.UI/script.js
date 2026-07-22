const map = L.map('map').setView([39.9351, 32.8435], 6);

L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    maxZoom: 19,
    attribution: '© OpenStreetMap'
}).addTo(map);

let currentPolylines = [];
let currentMarkers = [];

async function fetchRoute() {
    const locoId = document.getElementById('locoId').value;
    const startDate = document.getElementById('startDate').value;
    const endDate = document.getElementById('endDate').value;
    const btn = document.getElementById('drawBtn');

    if (!locoId || !startDate || !endDate) {
        alert("Lütfen tüm alanları doldurun.");
        return;
    }

    const url = `http://localhost:5215/api/telemetry?startDate=${startDate}&endDate=${endDate}&locomotiveId=${locoId}`;

    try {
        btn.textContent = "Veriler Çekiliyor...";
        btn.disabled = true;

        const response = await fetch(url);
        if (!response.ok) throw new Error("HTTP Hatası: " + response.status);

        const data = await response.json();

        const segments = data.segments || data.Segments || [];
        const telemetryList = data.telemetryData || data.TelemetryData || [];

        if (segments.length === 0 && telemetryList.length === 0) {
            alert("Kayıt bulunamadı.");
            return;
        }

        currentPolylines.forEach(p => map.removeLayer(p));
        currentPolylines = [];
        currentMarkers.forEach(m => map.removeLayer(m));
        currentMarkers = [];

        segments.forEach(segment => {
            const isGap = segment.isGap !== undefined ? segment.isGap : segment.IsGap;
            const coordsList = segment.coordinates || segment.Coordinates || [];

            if (coordsList.length > 0) {
                const latLngs = coordsList.map(w => [w.latitude || w.Latitude, w.longitude || w.Longitude]);

                let polyline;
                if (isGap) {
                    polyline = L.polyline(latLngs, { color: 'gray', weight: 4, dashArray: '10, 10' }).addTo(map);
                } else {
                    polyline = L.polyline(latLngs, { color: 'blue', weight: 4 }).addTo(map);
                }
                currentPolylines.push(polyline);
            }
        });

        if (currentPolylines.length > 0) {
            const group = new L.featureGroup(currentPolylines);
            map.fitBounds(group.getBounds());
        }

        const knownStops = [];
        telemetryList.forEach(point => {
            const speed = point.speed !== undefined ? point.speed : point.Speed;
            const lat = point.latitude || point.Latitude;
            const lon = point.longitude || point.Longitude;
            const time = point.recordedAt || point.RecordedAt;

            if (speed === 0) {
                const alreadyAdded = knownStops.some(
                    stop => Math.abs(stop.lat - lat) < 0.0001 && Math.abs(stop.lon - lon) < 0.0001
                );

                if (!alreadyAdded) {
                    knownStops.push({ lat: lat, lon: lon });
                    const marker = L.circleMarker([lat, lon], {
                        color: 'darkred', fillColor: 'red', fillOpacity: 1, radius: 6
                    }).addTo(map);

                    const timeString = new Date(time).toLocaleTimeString('tr-TR');
                    marker.bindPopup(`<b>İstasyon / Bekleme Noktası</b><br>Varış: ${timeString}`);
                    currentMarkers.push(marker);
                }
            }
        });

    } catch (error) {
        console.error("Hata:", error);
        alert("Veri çekilemedi.");
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