// scroll
window.scrollHelper = {
    onScrollEnd: function (dotnetObj) {
        let isLoading = false; // 🚫 Verhindert Wiederholung
        window.addEventListener("scroll", function () {

            const scrollPosition = window.innerHeight + window.scrollY;
            let pageHeight = document.body.scrollHeight;

            // Die Footer sollte nicht zur Seitenlänge gezählt werden.
            const footer = document.getElementById("page-footer");
            if (footer) {
                pageHeight -= footer.offsetHeight;
            }

            if (scrollPosition >= pageHeight - 2 && !isLoading) {
                isLoading = true;
                try {
                    dotnetObj.invokeMethodAsync("OnScrollEndReached");
                }
                catch (err) {
                    console.error("Fehler beim Aufrufen der .NET-Methode:", err);
                }
                finally {
                    setTimeout(() => {
                        isLoading = false;
                    }, 500);
                }
            }
        });
    }
};
// confirm
window.showConfirm = function (message) {
        return confirm(message);
}
// langauge cocikes fixd
window.blazorCulture = {
    set: function (value) {
        const days = 365; // 1 Jahr
        const date = new Date();
        date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
        const expires = "expires=" + date.toUTCString();

        document.cookie =
            `.AspNetCore.Culture=c=${value}|uic=${value}; ${expires}; path=/; SameSite=Lax`;
    }, get: function() {
        const name = ".AspNetCore.Culture=";
        const decodedCookie = decodeURIComponent(document.cookie);
        const cookies = decodedCookie.split(';');
        for (let i = 0; i < cookies.length; i++) {
            let c = cookies[i].trim();
            if (c.indexOf(name) === 0) {
                const value = c.substring(name.length);
                const cPart = value.split('|')[0];
                return cPart.split('=')[1];
            }
        }
        return null;
    }

};
// Cookie Consent 
window.cookieHelper = {
    set: function (name, value, days) {
        let expires = "";
        if (days) {
            const date = new Date();
            date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
            expires = "; expires=" + date.toUTCString();
        }
        document.cookie = `${name}=${value}; path=/; SameSite=Lax${expires}`;
    },
    get: function (name) {
        const cookies = document.cookie.split('; ');
        for (let c of cookies) {
            const [key, value] = c.split('=');
            if (key === name) return value;
        }
        return null;
    },

    delete: function (name) {
        document.cookie = `${name}=; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT; SameSite=Lax`;
    }
};
// script wenn Handy dann wird Whatsapp geöffent sonst wird webseite von Whatsapp geöffenet
window.whatsappRedirect = {
    openWhatsApp: function(phone, message) {
        const number = String(phone).replace(/\D/g, '');
        const text = message ? encodeURIComponent(message) : '';

        // linke
        const appUrl = text
            ? `whatsapp://send?phone=${number}&text=${text}`
            : `whatsapp://send?phone=${number}`;
        const webUrl = text
            ? `https://api.whatsapp.com/send?phone=${number}&text=${text}`
            : `https://wa.me/${number}`;

        // Gerättype prüfen
        const isMobile = /Android|iPhone|iPad|iPod|Opera Mini|IEMobile|WPDesktop/i.test(navigator.userAgent);

        if (isMobile) {
            // auf Handy versuche erst app zu öffnen
            window.location = appUrl;
        } else {
            // auf Computer app nicht möglich, öffne Web
            window.open(webUrl, '_blank');
        }
    }
};
// script wenn Handy wird MapApp geöffent sonst wird die Webseite von Google Map geöffenet
window.mapRedirect = {
    openMap: function(latitude, longitude, label = '') {
        latitude = String(latitude).trim();
        longitude = String(longitude).trim();
        label = String(label || '').trim();

        // Wenn ein Name vorhanden ist, verwenden wir diesen nur als Suchanfrage; andernfalls verwenden wir die Koordinaten.
        const query = label ? encodeURIComponent(label) : `${latitude},${longitude}`;

        // Links basierend auf Namen oder Koordinaten
        const appleUrl = `maps://?ll=${latitude},${longitude}`;
        const googleAppUrl = `comgooglemaps://?q=${query}`;
        const webUrl = `https://www.google.com/maps/search/?api=1&query=${query}`;
        const androidUrl = `geo:${latitude},${longitude}?q=${query}`;

        const ua = navigator.userAgent || window.opera;
        const isIOS = /iPad|iPhone|iPod/.test(ua) && !window.MSStream;
        const isAndroid = /Android/.test(ua);

        const openWebFallback = () => window.open(webUrl, '_blank');

        if (isIOS) {
            // iOS: Versuchen Sie es zuerst mit Apple Maps.
            window.location = appleUrl;

            // Gleich: Probieren Sie die Google Maps App aus.
            setTimeout(() => {
                window.location = googleAppUrl;

                // Nach einer Sekunde: Öffnen Sie das Web als letzte Option.
                setTimeout(() => openWebFallback(), 1000);
            }, 1000);

        } else if (isAndroid) {
            // Android: nutzen geo URI
            window.location = androidUrl;

            // Web-Fallback nach einer Sekunde
            setTimeout(() => openWebFallback(), 1000);

        } else {
            //Jedes andere Gerät → Web direkt öffnen
            openWebFallback();
        }
    }
};
// share product
window.shareProduct = async (title, url) => {
            try {
                if (navigator.share) {
                    await navigator.share({
                        title: title ?? '',
                        text: title ?? '',
                        url: url
                    });
                }
                else if (navigator.clipboard && window.isSecureContext) {
                    await navigator.clipboard.writeText(url);
                    console.log("تم نسخ الرابط بنجاح!");
                }
                else {
                    throw new Error("Clipboard API غير مدعوم أو الموقع غير آمن (HTTPS).");
                }
            }
            catch (error) {
                console.warn("Share/Copy failed:", error);
            }
        };
// Carousel automatisch laufen nach rended
window.initCarousel = function (carouselId) {
            var myCarousel = document.querySelector('#' + carouselId);

            if (!myCarousel) return;

            new bootstrap.Carousel(myCarousel, {
                interval: 5000,
                ride: 'carousel',
                pause: 'hover'
            });
        };
// Scroll sperren
window.openFullscreen = function() {
   document.body.style.overflow = 'hidden';
};

window.closeFullscreen = function () {
    document.body.style.overflow = '';
};
// header verschwenden und categoryNav ganz top verscheben
window.addEventListener('scroll', function () {
            const header = document.querySelector('.header');
            const nav = document.querySelector('.categoryNav');

            if (!header || !nav) return;

            const scrollTop = window.scrollY;
            const lastScrollTop = sessionStorage.getItem('lastScrollTop') || 0;

            if (scrollTop > lastScrollTop && scrollTop > 60) {
                header.classList.add('hidden');
                nav.classList.add('stick-to-top');
            }
            else {
                header.classList.remove('hidden');
                nav.classList.remove('stick-to-top');
            }

            sessionStorage.setItem('lastScrollTop', scrollTop);
        });
// Blazor.start
Blazor.start({
    ssr: {
        disablePrerendering: false // Für schnelleres Laden auf „false“ setzen
    },
    circuit: {
        reconnectionHandler: {
            onConnectionDown: (options, error) => {
                // Versuch, die Verbindung sofort und ohne Unterbrechung wiederherzustellen
                return new Promise(resolve => {
                    const attempt = () => {
                        if (navigator.onLine) {
                            resolve();
                        } else {
                            setTimeout(attempt, 1000);
                        }
                    };
                    attempt();
                });
            }
        }
    }
});

// Code zum Wiederverbinden nach der Rückkehr von WhatsApp oder aus dem Hintergrund
window.addEventListener('focus', async () => {
    try {
        // Versuch der manuellen Wiederverbindung
        await Blazor.reconnect();
    } catch (e) {
        console.log("Reconnection attempt failed, but Blazor will keep trying...");
    }
});