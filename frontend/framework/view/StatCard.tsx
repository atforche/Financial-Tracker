"use client";

import { Collapse, IconButton, Paper, Stack, Typography } from "@mui/material";
import type { JSX, ReactNode } from "react";
import usePageState, { isBoolean } from "@/framework/routes/usePageState";
import ExpandMore from "@mui/icons-material/ExpandMore";
import SignedChange from "@/framework/view/SignedChange";

/**
 * One row of a stat card's breakdown.
 */
interface StatCardDetail {
  readonly key: string;
  readonly label: string;
  readonly value: string;
  readonly change: number;
}

const noDetails: readonly StatCardDetail[] = [];

/**
 * Props for the StatCard component.
 */
interface StatCardProps {
  readonly label: string;
  readonly value: string;
  readonly caption: ReactNode;
  readonly details?: readonly StatCardDetail[];
  readonly detailsLabel?: string;
}

/**
 * Displays one headline number with a short label and caption, and an optional collapsible breakdown.
 */
const StatCard = function ({
  label,
  value,
  caption,
  details = noDetails,
  detailsLabel = "breakdown",
}: StatCardProps): JSX.Element {
  const [expanded, setExpanded] = usePageState(
    `statCard:${label}`,
    false,
    isBoolean,
  );
  return (
    <Paper
      sx={{
        // Keeps a card its own height when a neighbor's breakdown is expanded
        alignSelf: "start",
        border: "1px solid",
        borderColor: "divider",
        borderRadius: 3,
        p: { xs: 1.5, sm: 2 },
        display: "flex",
        flexDirection: "column",
        gap: 0.5,
        position: "relative",
      }}
    >
      <Stack spacing={0.5}>
        <Typography
          variant="body2"
          color="text.secondary"
          noWrap
          // Leaves room for the expand button in the corner
          sx={details.length === 0 ? {} : { pr: 3 }}
        >
          {label}
        </Typography>
        {details.length === 0 ? null : (
          <IconButton
            size="small"
            aria-expanded={expanded}
            aria-label={`${expanded ? "Hide" : "Show"} ${label.toLowerCase()} ${detailsLabel}`}
            onClick={() => {
              setExpanded((current) => !current);
            }}
            // Floats in the corner so cards with and without breakdowns line up
            sx={{ position: "absolute", top: 4, right: 4 }}
          >
            <ExpandMore
              sx={{
                transform: expanded ? "rotate(180deg)" : "none",
                transition: "transform 0.2s ease-in-out",
              }}
            />
          </IconButton>
        )}
        <Typography
          variant="h5"
          component="p"
          sx={{
            fontWeight: 500,
            fontSize: { xs: "1.1rem", sm: "1.5rem" },
            whiteSpace: "nowrap",
          }}
        >
          {value}
        </Typography>
        <Typography variant="body2" color="text.secondary">
          {caption}
        </Typography>
      </Stack>
      {details.length === 0 ? null : (
        <Collapse in={expanded} timeout="auto" unmountOnExit>
          <Stack spacing={1} sx={{ pt: 0.5 }}>
            {details.map((detail) => (
              // Half-width phone cards have no room beside the label, so the amount moves below it
              <Stack
                key={detail.key}
                direction={{ xs: "column", sm: "row" }}
                sx={{
                  columnGap: 1.5,
                  justifyContent: "space-between",
                  alignItems: { xs: "flex-start", sm: "baseline" },
                }}
              >
                <Typography variant="body2">{detail.label}</Typography>
                <Stack
                  sx={{ alignItems: { xs: "flex-start", sm: "flex-end" } }}
                >
                  <Typography
                    variant="body2"
                    sx={{ fontWeight: 500, whiteSpace: "nowrap" }}
                  >
                    {detail.value}
                  </Typography>
                  <Typography variant="caption" sx={{ whiteSpace: "nowrap" }}>
                    <SignedChange amount={detail.change} />
                  </Typography>
                </Stack>
              </Stack>
            ))}
          </Stack>
        </Collapse>
      )}
    </Paper>
  );
};

export default StatCard;
export type { StatCardDetail };
