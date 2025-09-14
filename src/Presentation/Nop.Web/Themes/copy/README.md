# GamingPCRehab - Modern Ecommerce Theme

A modern, responsive HTML theme designed for computer parts and gaming PC components ecommerce websites, inspired by professional sites like Spire.co.uk.

## 🚀 Features

- **Modern Dark Theme**: Professional dark color scheme with green accents
- **Fully Responsive**: Mobile-first design that works on all devices
- **Modular Structure**: Separate HTML files for each section for easy maintenance
- **Interactive Elements**: Hero slider, product tabs, and smooth animations
- **Ecommerce Ready**: Product cards, shopping cart, wishlist functionality
- **SEO Optimized**: Clean HTML structure and semantic markup
- **Fast Loading**: Optimized CSS and JavaScript

## 📁 File Structure

```
Theme/
├── index.html                 # Main page template
├── sections/                  # Individual section files
│   ├── header.html           # Navigation and header
│   ├── hero.html             # Hero slider section
│   ├── categories.html       # Product categories
│   ├── featured.html         # Featured products
│   └── footer.html           # Footer section
├── assets/
│   ├── css/
│   │   └── main.css          # Main stylesheet
│   ├── js/
│   │   └── main.js           # JavaScript functionality
│   └── images/               # Image assets
│       ├── products/         # Product images
│       ├── brands/           # Brand logos
│       ├── payments/         # Payment method icons
│       └── category/         # Category images
└── README.md                 # This file
```

## 🎨 Design Elements

### Color Scheme
- **Primary Background**: `#0a0a0a` (Deep Black)
- **Secondary Background**: `#111` (Dark Gray)
- **Accent Color**: `#00ff88` (Bright Green)
- **Text Primary**: `#ffffff` (White)
- **Text Secondary**: `#999` (Light Gray)

### Typography
- **Font Family**: Poppins (Google Fonts)
- **Font Weights**: 300, 400, 500, 600, 700

## 🔧 Setup Instructions

1. **Clone or Download** the theme files to your project directory

2. **Add Images**: Place your images in the appropriate directories:
   - Logo: `assets/images/logo.png` and `assets/images/logo-white.png`
   - Hero backgrounds: `assets/images/hero-bg-1.jpg`, `hero-bg-2.jpg`, etc.
   - Product images: `assets/images/products/`
   - Brand logos: `assets/images/brands/`
   - Category images: `assets/images/category/`
   - Payment icons: `assets/images/payments/`

3. **Customize Content**: Edit the HTML files in the `sections/` folder to match your products and branding

4. **Configure JavaScript**: Update `assets/js/main.js` to integrate with your ecommerce platform's API

## 📱 Responsive Breakpoints

- **Mobile**: < 768px
- **Tablet**: 768px - 1024px
- **Desktop**: > 1024px

## 🛠 Customization

### Changing Colors
Edit the CSS variables in `assets/css/main.css`:

```css
:root {
    --primary-color: #00ff88;
    --background-dark: #0a0a0a;
    --background-light: #111;
}
```

### Adding New Sections
1. Create a new HTML file in the `sections/` folder
2. Add a placeholder div in `index.html`
3. Update `assets/js/main.js` to load the new section

### Modifying Navigation
Edit `sections/header.html` to add/remove menu items and categories

## 🔌 Integration with Ecommerce Platforms

This theme is designed to be easily integrated with popular ecommerce platforms:

### Shopify
- Replace static content with Liquid tags
- Integrate with Shopify's cart and checkout system
- Use Shopify's image optimization

### WooCommerce
- Convert to PHP templates
- Integrate with WooCommerce hooks and filters
- Use WordPress media library

### Magento
- Convert to Magento 2 theme structure
- Integrate with Magento's layout system
- Use Magento's product catalog

## 📋 Required Images

To complete the theme setup, you'll need the following images:

### Logo & Branding
- `logo.png` - Main logo (dark background)
- `logo-white.png` - White logo (light background)

### Hero Section
- `hero-bg-1.jpg` - Hero background 1 (1920x600px)
- `hero-bg-2.jpg` - Hero background 2 (1920x600px)
- `hero-bg-3.jpg` - Hero background 3 (1920x600px)
- `hero-product.png` - Hero product image
- `rtx-4090.png` - RTX 4090 product image
- `custom-build.png` - Custom build image

### Categories
- `category-gpu.jpg` - Graphics cards category
- `category-cpu.jpg` - Processors category
- `category-motherboard.jpg` - Motherboards category
- `category-ram.jpg` - Memory category
- `category-storage.jpg` - Storage category
- `category-cooling.jpg` - Cooling category
- `category-psu.jpg` - Power supplies category
- `category-cases.jpg` - PC cases category

### Products
- Various product images in `products/` folder
- Recommended size: 400x400px

### Brands
- Brand logos in `brands/` folder
- Recommended size: 120x50px

### Payment Methods
- Payment method icons in `payments/` folder
- Recommended size: 60x30px

## 🚀 Performance Optimization

- **Image Optimization**: Use WebP format when possible
- **Lazy Loading**: Images load only when visible
- **Minification**: Minify CSS and JavaScript for production
- **CDN**: Use a CDN for faster asset delivery

## 📞 Support

For customization help or questions about integrating this theme with your ecommerce platform, please refer to the documentation of your chosen platform or consult with a web developer familiar with ecommerce implementations.

## 📄 License

This theme is provided as-is for educational and commercial use. Feel free to modify and adapt it to your needs.

---

**Built for GamingPCRehab** - Premium Gaming PC Components