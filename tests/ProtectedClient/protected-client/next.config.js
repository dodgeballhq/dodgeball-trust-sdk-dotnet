/** @type {import('next').NextConfig} */
const nextConfig = {
  reactStrictMode: true,
   async rewrites() {
    return [
      {
        source: '/api/:path*',
        destination: 'http://localhost:5125/:path*'
      }
    ]
  }
}

module.exports = nextConfig
