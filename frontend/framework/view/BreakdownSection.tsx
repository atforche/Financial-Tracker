import { Box, Collapse, IconButton, Stack, Typography } from "@mui/material";
import type { JSX, ReactNode } from "react";
import ExpandMore from "@mui/icons-material/ExpandMore";

/**
 * Represents a single row of detail information in the BreakdownSection component.
 */
interface BreakdownDetailRow {
  readonly key: string;
  readonly label: string;
  readonly value: ReactNode;
}

/**
 * Props for the BreakdownSection component.
 */
interface BreakdownSectionProps {
  readonly label: string;
  readonly value: ReactNode;
  readonly detailRows?: readonly BreakdownDetailRow[];
  readonly expanded?: boolean;
  readonly onToggle?: () => void;
}

const toggleSlotSize = 26;
const noDetailRows: readonly BreakdownDetailRow[] = [];

/**
 * Displays a labeled value with optional expandable detail rows.
 */
const BreakdownSection = function ({
  label,
  value,
  detailRows = noDetailRows,
  expanded = false,
  onToggle,
}: BreakdownSectionProps): JSX.Element {
  const hasDetails = detailRows.length > 0 && typeof onToggle !== "undefined";
  return (
    <Stack spacing={0.75}>
      <Stack
        direction="row"
        justifyContent="space-between"
        alignItems="center"
        gap={1.5}
      >
        <Typography variant="body2">{label}</Typography>
        <Stack direction="row" alignItems="center" gap={0.5}>
          <Typography
            variant="body2"
            fontWeight={600}
            sx={{ textAlign: "right" }}
          >
            {value}
          </Typography>
          {typeof onToggle !== "undefined" && (
            <Box
              sx={{
                width: toggleSlotSize,
                height: toggleSlotSize,
                display: "flex",
                alignItems: "center",
                justifyContent: "center",
              }}
            >
              {hasDetails ? (
                <IconButton
                  size="small"
                  onClick={onToggle}
                  aria-label={`${expanded ? "Collapse" : "Expand"} ${label}`}
                  sx={{
                    p: 0.25,
                    transform: expanded ? "rotate(180deg)" : "rotate(0deg)",
                    transition: "transform 0.3s ease-in-out",
                  }}
                >
                  <ExpandMore fontSize="small" />
                </IconButton>
              ) : null}
            </Box>
          )}
        </Stack>
      </Stack>
      {hasDetails ? (
        <Collapse in={expanded} timeout="auto" unmountOnExit>
          <Stack spacing={0.75} sx={{ pl: 1.5 }}>
            {detailRows.map((row) => (
              <Stack
                key={row.key}
                direction="row"
                justifyContent="space-between"
                alignItems="baseline"
                gap={1.5}
              >
                <Typography variant="caption" color="text.secondary">
                  {row.label}
                </Typography>
                <Stack direction="row" alignItems="center" gap={0.5}>
                  <Typography
                    variant="caption"
                    fontWeight={600}
                    sx={{ textAlign: "right" }}
                  >
                    {row.value}
                  </Typography>
                  <Box
                    sx={{
                      width: toggleSlotSize,
                      height: toggleSlotSize,
                      flexShrink: 0,
                    }}
                  />
                </Stack>
              </Stack>
            ))}
          </Stack>
        </Collapse>
      ) : null}
    </Stack>
  );
};

export type { BreakdownDetailRow };
export default BreakdownSection;
