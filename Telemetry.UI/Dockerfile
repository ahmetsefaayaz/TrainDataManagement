# En hafif Nginx imajını kullan
FROM nginx:alpine

# Nginx'in varsayılan statik dosya dizinine kendi dosyalarımızı kopyalayalım
COPY Telemetry.UI/index.html /usr/share/nginx/html/
COPY Telemetry.UI/script.js /usr/share/nginx/html/

# Nginx varsayılan olarak 80 portunda çalışır
EXPOSE 80