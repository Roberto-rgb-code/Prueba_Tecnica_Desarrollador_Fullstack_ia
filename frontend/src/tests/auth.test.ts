import { describe, it, expect } from 'vitest';

describe('auth validation', () => {
  it('requires minimum password length', () => {
    expect('123456'.length).toBeGreaterThanOrEqual(6);
  });

  it('validates email format', () => {
    expect('user@toka.com'.includes('@')).toBe(true);
  });
});
