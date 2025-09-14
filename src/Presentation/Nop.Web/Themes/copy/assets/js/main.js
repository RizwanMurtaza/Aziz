// Initialize functionality when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    initHeroSlider();
    initProductTabs();
    initMobileMenu();
    initFeaturesToggle();
});

// Mobile Menu Functionality
function initMobileMenu() {
    const mobileMenuToggle = document.querySelector('.mobile-menu-toggle');
    const megaMenu = document.querySelector('.mega-menu');
    
    if (mobileMenuToggle && megaMenu) {
        mobileMenuToggle.addEventListener('click', function(e) {
            e.stopPropagation();
            // Toggle mobile menu
            if (window.innerWidth <= 768) {
                megaMenu.classList.toggle('show');
                mobileMenuToggle.classList.toggle('active');
            }
        });
        
        // Close mobile menu when clicking outside
        document.addEventListener('click', function(e) {
            if (window.innerWidth <= 768 && 
                !megaMenu.contains(e.target) && 
                !mobileMenuToggle.contains(e.target)) {
                megaMenu.classList.remove('show');
                mobileMenuToggle.classList.remove('active');
            }
        });
        
        // Close menu when clicking on a menu item
        const menuLinks = megaMenu.querySelectorAll('a');
        menuLinks.forEach(link => {
            link.addEventListener('click', function() {
                if (window.innerWidth <= 768) {
                    megaMenu.classList.remove('show');
                    mobileMenuToggle.classList.remove('active');
                }
            });
        });
        
        // Handle window resize
        window.addEventListener('resize', function() {
            if (window.innerWidth > 768) {
                megaMenu.classList.remove('show');
                mobileMenuToggle.classList.remove('active');
            } else {
                megaMenu.classList.remove('show');
                mobileMenuToggle.classList.remove('active');
            }
        });
    }
}

// Features Toggle Functionality
function initFeaturesToggle() {
    const featuresToggle = document.querySelector('.features-toggle');
    const heroFeatures = document.querySelector('.hero-features');
    const featuresClose = document.querySelector('.features-close');
    const featuresOverlay = document.querySelector('.features-overlay');
    
    if (featuresToggle && heroFeatures) {
        // Open features panel
        featuresToggle.addEventListener('click', function() {
            if (window.innerWidth <= 768) {
                heroFeatures.classList.add('show');
                featuresToggle.classList.add('active');
                featuresOverlay.classList.add('show');
                document.body.style.overflow = 'hidden'; // Prevent body scroll
            }
        });
        
        // Close features panel
        function closeFeatures() {
            heroFeatures.classList.remove('show');
            featuresToggle.classList.remove('active');
            featuresOverlay.classList.remove('show');
            document.body.style.overflow = ''; // Restore body scroll
        }
        
        // Close button click
        if (featuresClose) {
            featuresClose.addEventListener('click', closeFeatures);
        }
        
        // Overlay click
        if (featuresOverlay) {
            featuresOverlay.addEventListener('click', closeFeatures);
        }
        
        // Handle window resize
        window.addEventListener('resize', function() {
            if (window.innerWidth > 768) {
                closeFeatures();
            }
        });
    }
}

// Hero Slider Functionality
function initHeroSlider() {
    let currentSlide = 0;
    const slides = document.querySelectorAll('.hero-slide');
    const dots = document.querySelectorAll('.dot');
    const nextBtn = document.querySelector('.next-slide');
    const prevBtn = document.querySelector('.prev-slide');

    function showSlide(index) {
        slides.forEach(slide => slide.classList.remove('active'));
        dots.forEach(dot => dot.classList.remove('active'));
        
        slides[index].classList.add('active');
        dots[index].classList.add('active');
    }

    function nextSlide() {
        currentSlide = (currentSlide + 1) % slides.length;
        showSlide(currentSlide);
    }

    function prevSlide() {
        currentSlide = (currentSlide - 1 + slides.length) % slides.length;
        showSlide(currentSlide);
    }

    // Auto-play slider
    setInterval(nextSlide, 5000);

    // Event listeners
    if (nextBtn) nextBtn.addEventListener('click', nextSlide);
    if (prevBtn) prevBtn.addEventListener('click', prevSlide);

    dots.forEach((dot, index) => {
        dot.addEventListener('click', () => {
            currentSlide = index;
            showSlide(currentSlide);
        });
    });
}

// Product Tabs Functionality
function initProductTabs() {
    const tabItems = document.querySelectorAll('.tab-item a');
    const tabContents = document.querySelectorAll('.tab-content');

    tabItems.forEach(tab => {
        tab.addEventListener('click', (e) => {
            e.preventDefault();
            
            // Remove active class from all tabs and contents
            tabItems.forEach(item => item.parentElement.classList.remove('active'));
            tabContents.forEach(content => content.classList.remove('active'));
            
            // Add active class to clicked tab
            tab.parentElement.classList.add('active');
            
            // Show corresponding content
            const targetTab = tab.getAttribute('data-tab');
            document.getElementById(targetTab).classList.add('active');
        });
    });
}


// Search functionality
document.addEventListener('submit', function(e) {
    if (e.target.classList.contains('search-form')) {
        e.preventDefault();
        const searchTerm = e.target.querySelector('.search-input').value;
        console.log('Searching for:', searchTerm);
        // Add your search logic here
    }
});

// Newsletter form
document.addEventListener('submit', function(e) {
    if (e.target.classList.contains('newsletter-form')) {
        e.preventDefault();
        const email = e.target.querySelector('.newsletter-input').value;
        console.log('Newsletter signup:', email);
        // Add your newsletter signup logic here
        alert('Thank you for subscribing to our newsletter!');
    }
});

// Add to cart functionality
document.addEventListener('click', function(e) {
    if (e.target.classList.contains('add-to-cart')) {
        e.preventDefault();
        console.log('Adding to cart...');
        // Add your cart logic here
        
        // Visual feedback
        const button = e.target;
        const originalText = button.textContent;
        button.textContent = 'Added!';
        button.style.background = '#28a745';
        
        setTimeout(() => {
            button.textContent = originalText;
            button.style.background = '';
        }, 2000);
    }
});

// Wishlist functionality
document.addEventListener('click', function(e) {
    if (e.target.classList.contains('add-wishlist') || e.target.parentElement.classList.contains('add-wishlist')) {
        e.preventDefault();
        const button = e.target.classList.contains('add-wishlist') ? e.target : e.target.parentElement;
        const icon = button.querySelector('i');
        
        if (icon.classList.contains('fas')) {
            icon.classList.remove('fas');
            icon.classList.add('far');
            button.style.color = '#999';
        } else {
            icon.classList.remove('far');
            icon.classList.add('fas');
            button.style.color = '#ff4444';
        }
    }
});

// Smooth scrolling for anchor links
document.addEventListener('click', function(e) {
    if (e.target.tagName === 'A' && e.target.getAttribute('href').startsWith('#')) {
        e.preventDefault();
        const targetId = e.target.getAttribute('href');
        const targetElement = document.querySelector(targetId);
        if (targetElement) {
            targetElement.scrollIntoView({ behavior: 'smooth' });
        }
    }
});

// Lazy loading for images
document.addEventListener('DOMContentLoaded', function() {
    const images = document.querySelectorAll('img[data-src]');
    
    const imageObserver = new IntersectionObserver((entries, observer) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const img = entry.target;
                img.src = img.dataset.src;
                img.classList.remove('lazy');
                imageObserver.unobserve(img);
            }
        });
    });

    images.forEach(img => imageObserver.observe(img));
});

// Scroll to top functionality
window.addEventListener('scroll', function() {
    const scrollTop = document.documentElement.scrollTop || document.body.scrollTop;
    
    // Show/hide scroll to top button
    let scrollTopBtn = document.querySelector('.scroll-to-top');
    if (!scrollTopBtn) {
        scrollTopBtn = document.createElement('button');
        scrollTopBtn.className = 'scroll-to-top';
        scrollTopBtn.innerHTML = '<i class="fas fa-arrow-up"></i>';
        scrollTopBtn.style.cssText = `
            position: fixed;
            bottom: 30px;
            right: 30px;
            background: linear-gradient(135deg, #00ff88, #00cc6a);
            color: #000;
            border: none;
            width: 50px;
            height: 50px;
            border-radius: 50%;
            cursor: pointer;
            display: none;
            z-index: 1000;
            transition: all 0.3s ease;
        `;
        document.body.appendChild(scrollTopBtn);
        
        scrollTopBtn.addEventListener('click', () => {
            window.scrollTo({ top: 0, behavior: 'smooth' });
        });
    }
    
    if (scrollTop > 300) {
        scrollTopBtn.style.display = 'flex';
        scrollTopBtn.style.alignItems = 'center';
        scrollTopBtn.style.justifyContent = 'center';
    } else {
        scrollTopBtn.style.display = 'none';
    }
});