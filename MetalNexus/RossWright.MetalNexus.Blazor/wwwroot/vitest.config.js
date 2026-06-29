import { defineConfig } from 'vitest/config';

export default defineConfig({
    test: {
        environment: 'jsdom',
        globals: true,
        include: ['**/*.test.js'],
        coverage: {
            provider: 'v8',
            include: ['Interop.js'],
            reporter: ['text', 'lcov'],
        },
    },
});
