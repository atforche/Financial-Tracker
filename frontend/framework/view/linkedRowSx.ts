/**
 * Highlights a table row whose link covers the whole row.
 */
const linkedRowSx = {
  position: "relative",
  "&:hover": { bgcolor: "action.hover" },
} as const;

/**
 * Stretches a link in a table row across the whole row.
 */
const rowLinkSx = {
  color: "inherit",
  textDecoration: "none",
  "&::after": { content: '""', position: "absolute", inset: 0 },
} as const;

export { linkedRowSx, rowLinkSx };
