/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./**/*.{razor,html,cshtml}",
    "../GmGard/Views/**/*.cshtml"
  ],
  theme: {
    extend: {
      colors: {
        'primary': '#1b6ec2',
        'primary-dark': '#053967',
        'accent': '#3a0647',
      }
    },
  },
  plugins: [],
}
