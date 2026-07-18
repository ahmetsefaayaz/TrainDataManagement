const map = L.map('map').setView([39.9351, 32.8435], 6);

L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    maxZoom: 19,
    attribution: '© OpenStreetMap'
}).addTo(map);

let currentPolyline = null;
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
        currentMarkers.forEach(marker => map.removeLayer(marker));
        currentMarkers = []; 

        currentPolyline = L.polyline(latLngs, { color: 'blue', weight: 4 }).addTo(map);

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