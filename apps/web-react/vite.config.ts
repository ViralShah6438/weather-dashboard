import { defineConfig, loadEnv } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '');

  return {
    plugins: [react()],
    server: {
      port: 4202,
      proxy: {
        '/api': {
          target: env.VITE_API_ORIGIN ?? 'https://localhost:52938',
          changeOrigin: true,
          secure: false
        }
      }
    },
    test: {
      globals: true,
      environment: 'jsdom',
      setupFiles: './src/test/setup.ts',
      css: true
    }
  };
});
