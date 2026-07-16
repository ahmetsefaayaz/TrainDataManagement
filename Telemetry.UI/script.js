// Haritayı başlat (Ankara merkezli)
const map = L.map('map').setView([39.9207, 32.8541], 6);

L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    maxZoom: 19,
    attribution: '© OpenStreetMap'
}).addTo(map);

let currentPolyline = null;

async function fetchRoute() {
    const locoId = document.getElementById('locoId').value;
    const startDate = document.getElementById('startDate').value;
    const endDate = document.getElementById('endDate').value;
    const btn = document.getElementById('drawBtn');

    // ID Kontrolü (Artık zorunlu)
    if (!locoId) {
        alert("Lütfen geçerli bir Lokomotif ID girin! (Örn: 1)");
        return;
    }

    if (!startDate || !endDate) {
        alert("Lütfen başlangıç ve bitiş tarihlerini seçin.");
        return;
    }

    // Portun 5215 olduğuna dikkat et
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

        // Sadece belirtilen ID'ye ait olan koordinatları dizi haline getir
        const latLngs = data.map(point => [point.latitude, point.longitude]);

        // Eski çizgi varsa temizle
        if (currentPolyline) {
            map.removeLayer(currentPolyline);
        }

        // Yeni rotayı mavi, kalın bir çizgi olarak haritaya bas
        currentPolyline = L.polyline(latLngs, { color: 'blue', weight: 4 }).addTo(map);

        // Harita kamerasını çizilen yola otomatik odakla
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

// Sayfa açıldığında tarihleri otomatik doldur
document.addEventListener("DOMContentLoaded", () => {
    const now = new Date();
    const oneHourAgo = new Date(now.getTime() - (60 * 60 * 1000));

    // ISO formatına çevir (YYYY-MM-DDThh:mm)
    document.getElementById('startDate').value = oneHourAgo.toISOString().slice(0, 16);
    document.getElementById('endDate').value = now.toISOString().slice(0, 16);
});