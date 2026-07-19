interface CollectionPage<T> {
  readonly items: T[];
  readonly totalCount: number;
}

const pageSize = 500;

/**
 * Loads every page from an API collection without relying on a fixed cap.
 */
const loadAllPages = async function <T>(
  loadPage: (limit: number, offset: number) => Promise<CollectionPage<T>>,
): Promise<T[]> {
  const loadRemainingPages = async function (items: T[]): Promise<T[]> {
    const page = await loadPage(pageSize, items.length);
    const nextItems = [...items, ...page.items];
    return nextItems.length >= page.totalCount || page.items.length === 0
      ? nextItems
      : loadRemainingPages(nextItems);
  };
  return loadRemainingPages([]);
};

export default loadAllPages;
