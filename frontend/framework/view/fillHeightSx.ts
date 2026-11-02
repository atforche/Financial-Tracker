/**
 * Stretches a section's surface to the height of its row so side-by-side sections line up.
 */
const fillHeightSx = {
  display: "flex",
  flexDirection: "column",
  "& > .MuiPaper-root": { flex: 1 },
} as const;

export default fillHeightSx;
