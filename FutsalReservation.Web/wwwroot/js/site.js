// Toast / flash message: auto hilang setelah beberapa detik, bisa ditutup manual
(function () {
    function tutup(toast) {
        toast.classList.add('keluar');
        setTimeout(function () {
            if (toast.parentNode) {
                toast.parentNode.removeChild(toast);
            }
        }, 250);
    }

    function pasang() {
        var daftar = document.querySelectorAll('[data-toast]');
        daftar.forEach(function (toast) {
            // tampilkan dengan animasi masuk
            requestAnimationFrame(function () {
                toast.classList.add('tampil');
            });

            var tombol = toast.querySelector('.toast-tutup');
            if (tombol) {
                tombol.addEventListener('click', function () {
                    tutup(toast);
                });
            }

            // toast error bertahan lebih lama
            var durasi = toast.classList.contains('toast-error') ? 6000 : 4000;
            setTimeout(function () {
                tutup(toast);
            }, durasi);
        });
    }

    if (document.readyState !== 'loading') {
        pasang();
    } else {
        document.addEventListener('DOMContentLoaded', pasang);
    }
})();

// Tema terang/gelap: simpan pilihan di localStorage
(function () {
    function terapkan(tema) {
        document.documentElement.setAttribute('data-theme', tema);
        try { localStorage.setItem('tema', tema); } catch (e) { }
    }

    function pasang() {
        var tombol = document.querySelectorAll('.theme-toggle');
        tombol.forEach(function (b) {
            b.addEventListener('click', function () {
                var sekarang = document.documentElement.getAttribute('data-theme') === 'dark' ? 'light' : 'dark';
                terapkan(sekarang);
            });
        });
    }

    if (document.readyState !== 'loading') {
        pasang();
    } else {
        document.addEventListener('DOMContentLoaded', pasang);
    }
})();

// Modal konfirmasi (pengganti confirm bawaan browser) + lightbox gambar
(function () {
    var overlayKonfirmasi = document.getElementById('modalKonfirmasi');
    var pesanEl = document.getElementById('modalPesan');
    var okeBtn = document.getElementById('modalOke');
    var overlayGambar = document.getElementById('modalGambar');
    var gambarSrc = document.getElementById('modalGambarSrc');
    var formTertunda = null;

    function buka(o) {
        if (!o) return;
        o.classList.add('terbuka');
        requestAnimationFrame(function () { o.classList.add('tampil'); });
    }

    function tutup(o) {
        if (!o) return;
        o.classList.remove('tampil');
        setTimeout(function () { o.classList.remove('terbuka'); }, 180);
    }

    // minta konfirmasi sebelum submit form yang punya atribut data-confirm
    document.addEventListener('submit', function (e) {
        var form = e.target;
        if (form && form.hasAttribute && form.hasAttribute('data-confirm') && form.dataset.dikonfirmasi !== '1') {
            e.preventDefault();
            formTertunda = form;
            if (pesanEl) pesanEl.textContent = form.getAttribute('data-confirm');
            if (okeBtn) {
                okeBtn.textContent = form.getAttribute('data-confirm-ok') || 'Ya, Lanjutkan';
                okeBtn.className = 'btn ' + (form.getAttribute('data-confirm-variant') === 'danger' ? 'btn-danger-solid' : 'btn-primary');
            }
            buka(overlayKonfirmasi);
        }
    });

    if (okeBtn) {
        okeBtn.addEventListener('click', function () {
            if (!formTertunda) return;
            var f = formTertunda;
            formTertunda = null;
            f.dataset.dikonfirmasi = '1';
            tutup(overlayKonfirmasi);
            f.submit();
        });
    }

    // buka gambar (bukti transfer) dalam lightbox
    document.addEventListener('click', function (e) {
        var pemicu = e.target.closest ? e.target.closest('[data-img]') : null;
        if (pemicu) {
            e.preventDefault();
            if (gambarSrc) gambarSrc.src = pemicu.getAttribute('data-img');
            buka(overlayGambar);
        }
    });

    // tutup modal: tombol batal/close atau klik area gelap
    document.addEventListener('click', function (e) {
        if (e.target.closest && e.target.closest('[data-modal-batal]')) {
            tutup(overlayKonfirmasi); tutup(overlayGambar); formTertunda = null;
        } else if (e.target.classList && e.target.classList.contains('modal-overlay')) {
            tutup(e.target); formTertunda = null;
        }
    });

    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') { tutup(overlayKonfirmasi); tutup(overlayGambar); formTertunda = null; }
    });
})();
