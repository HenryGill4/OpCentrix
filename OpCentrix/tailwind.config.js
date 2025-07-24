/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    './Pages/**/*.cshtml',
    './Views/**/*.cshtml',
    './wwwroot/js/**/*.js',
    './Models/**/*.cs'
  ],
  theme: {
    extend: {
      colors: {
        'opcentrix': {
          'primary': '#3B82F6',
          'primary-dark': '#1E40AF',
          'secondary': '#6366F1',
          'accent': '#10B981',
          'accent-dark': '#047857',
          'success': '#10B981',
          'warning': '#F59E0B',
          'danger': '#EF4444',
          'gray': {
            '50': '#F9FAFB',
            '100': '#F3F4F6',
            '200': '#E5E7EB',
            '300': '#D1D5DB',
            '400': '#9CA3AF',
            '500': '#6B7280',
            '600': '#4B5563',
            '700': '#374151',
            '800': '#1F2937',
            '900': '#111827'
          }
        }
      },
      borderRadius: {
        'opcentrix': '8px',
        'opcentrix-sm': '6px',
        'opcentrix-lg': '12px',
        'opcentrix-xl': '16px'
      },
      boxShadow: {
        'opcentrix': '0 1px 3px 0 rgba(0, 0, 0, 0.1), 0 1px 2px 0 rgba(0, 0, 0, 0.06)',
        'opcentrix-lg': '0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05)',
        'opcentrix-xl': '0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04)'
      },
      fontFamily: {
        'sans': ['Inter', 'ui-sans-serif', 'system-ui', '-apple-system', 'BlinkMacSystemFont', 'Segoe UI', 'Roboto', 'Helvetica Neue', 'Arial', 'Noto Sans', 'sans-serif'],
        'mono': ['JetBrains Mono', 'ui-monospace', 'SFMono-Regular', 'Menlo', 'Monaco', 'Consolas', 'Liberation Mono', 'Courier New', 'monospace']
      },
      animation: {
        'slide-in-right': 'slideInRight 0.3s ease-out',
        'slide-out-right': 'slideOutRight 0.3s ease-in',
        'fade-in': 'fadeIn 0.3s ease-out'
      },
      gridTemplateColumns: {
        'scheduler': 'minmax(140px, 160px) repeat(auto-fit, minmax(120px, 1fr))',
        'scheduler-compact': 'minmax(100px, 120px) repeat(auto-fit, minmax(80px, 1fr))',
        'scheduler-mobile': 'minmax(80px, 100px) repeat(auto-fit, minmax(60px, 1fr))'
      }
    }
  },
  plugins: [
    require('@tailwindcss/forms'),
    require('@tailwindcss/typography')
  ]
}