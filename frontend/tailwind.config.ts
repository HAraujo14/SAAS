import type { Config } from 'tailwindcss'

const config: Config = {
  // Apenas os ficheiros que referenciam classes Tailwind — evita CSS desnecessário no bundle
  content: ['./index.html', './src/**/*.{ts,tsx}'],
  theme: {
    extend: {
      colors: {
        // Paleta de marca do AprovaFlow — alterar aqui muda em toda a app
        brand: {
          50: '#eff6ff',
          100: '#dbeafe',
          500: '#3b82f6',
          600: '#2563eb',
          700: '#1d4ed8',
          900: '#1e3a8a',
        },
      },
      fontFamily: {
        sans: ['Inter', 'system-ui', 'sans-serif'],
      },
    },
  },
  plugins: [],
}

export default config
