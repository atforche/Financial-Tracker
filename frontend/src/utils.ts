const ensureNotNull = function<T>(item: T | null, message?: string): T {
  if (item === null) {
    throw new Error(message ?? 'item is undefined');
  }
  return item;
};

export { ensureNotNull }