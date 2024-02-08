document.addEventListener('DOMContentLoaded', function () {   
    const activeClass = 'is-active';
    const navbarBurger = document.getElementById('NavbarBurger');

    if (navbarBurger) {
        navbarBurger.addEventListener('click', function () {
            const mainNavbar = document.getElementById(navbarBurger.dataset.target);

            navbarBurger.classList.toggle(activeClass);
            mainNavbar.classList.toggle(activeClass);
        });
    }    
});