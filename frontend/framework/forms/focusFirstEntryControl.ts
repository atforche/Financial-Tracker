/**
 * Focuses the first entry control in the container.
 */
export const focusFirstEntryControl = function (
  container: HTMLElement | null,
): void {
  if (container === null) {
    return;
  }

  requestAnimationFrame(() => {
    const firstControl = container.querySelector<HTMLElement>(
      'input:not([type="hidden"]):not([disabled]), textarea:not([disabled]), select:not([disabled]), [role="combobox"]',
    );

    firstControl?.focus();
  });
};
