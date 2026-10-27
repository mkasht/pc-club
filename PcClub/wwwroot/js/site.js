(() => {
    'use strict';
    const menu = document.getElementById('mobile-menu');
    const toggle = document.querySelector('.menu-toggle');
    if (menu && toggle) {
        toggle.addEventListener('click', () => {
            menu.showModal();
            toggle.setAttribute('aria-expanded', 'true');
        });
        menu.querySelector('[data-close-menu]').addEventListener('click', () => menu.close());
        menu.querySelectorAll('a').forEach((link) =>
            link.addEventListener('click', () => menu.close()),
        );
        menu.addEventListener('close', () => {
            toggle.setAttribute('aria-expanded', 'false');
            toggle.focus({ preventScroll: true });
        });
    }
    const tabs = [...document.querySelectorAll('[data-zone-tab]')];
    const selectTab = (tab) => {
        tabs.forEach((item) => {
            const active = item === tab;
            item.setAttribute('aria-selected', String(active));
            item.tabIndex = active ? 0 : -1;
            document.getElementById(item.getAttribute('aria-controls')).hidden = !active;
        });
    };
    tabs.forEach((tab, index) => {
        tab.addEventListener('click', () => selectTab(tab));
        tab.addEventListener('keydown', (event) => {
            let next;
            if (event.key === 'ArrowRight') next = (index + 1) % tabs.length;
            if (event.key === 'ArrowLeft') next = (index - 1 + tabs.length) % tabs.length;
            if (event.key === 'Home') next = 0;
            if (event.key === 'End') next = tabs.length - 1;
            if (next === undefined) return;
            event.preventDefault();
            selectTab(tabs[next]);
            tabs[next].focus();
        });
    });
    const gallery = document.getElementById('gallery-dialog');
    let galleryTrigger;
    if (gallery) {
        document.querySelectorAll('[data-gallery]').forEach((button) =>
            button.addEventListener('click', () => {
                galleryTrigger = button;
                const image = gallery.querySelector('img');
                const thumbnail =
                    button.querySelector('img') ||
                    button.closest('.zone-photo, .detail-photo')?.querySelector('img');
                image.src = thumbnail?.currentSrc || button.dataset.gallery;
                image.alt = button.dataset.caption || 'Интерьер клуба';
                gallery.querySelector('p').textContent = image.alt;
                gallery.showModal();
            }),
        );
        gallery
            .querySelector('[data-close-gallery]')
            .addEventListener('click', () => gallery.close());
        gallery.addEventListener('click', (event) => {
            if (event.target === gallery && event.offsetY < 0) gallery.close();
        });
        gallery.addEventListener('close', () => galleryTrigger?.focus({ preventScroll: true }));
    }
    document
        .querySelectorAll('[data-dismiss]')
        .forEach((button) => button.addEventListener('click', () => button.parentElement.remove()));
    document.querySelectorAll('form').forEach((form) =>
        form.addEventListener('submit', (event) => {
            if (form.dataset.confirm && !window.confirm(form.dataset.confirm)) {
                event.preventDefault();
                return;
            }
            const button = event.submitter;
            if (button && !event.defaultPrevented) {
                button.disabled = true;
                button.textContent = 'Подождите…';
            }
        }),
    );
    const imageInput = document.querySelector('[data-image-input]');
    let previewUrl;
    imageInput?.addEventListener('change', () => {
        const file = imageInput.files?.[0];
        imageInput.setCustomValidity('');
        if (!file) return;
        if (file.size > 4 * 1024 * 1024) {
            imageInput.setCustomValidity('Размер изображения должен быть не больше 4 МБ.');
            imageInput.reportValidity();
            return;
        }
        imageInput.setCustomValidity('');
        if (previewUrl) URL.revokeObjectURL(previewUrl);
        previewUrl = URL.createObjectURL(file);
        document.querySelector('[data-image-preview]').src = previewUrl;
    });
    document.querySelectorAll('img').forEach((image) =>
        image.addEventListener(
            'error',
            () => {
                if (image.getAttribute('src') !== '/images/arena.webp')
                    image.src = '/images/arena.webp';
            },
            { once: true },
        ),
    );
})();
