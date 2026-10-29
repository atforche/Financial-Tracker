"use client";

import { Collapse, IconButton, Paper, Stack, Typography } from "@mui/material";
import { type JSX, type ReactNode, useState } from "react";
import ExpandMore from "@mui/icons-material/ExpandMore";
import OverviewChange from "@/overview/OverviewChange";

/**
 * One row of an overview stat's breakdown.
 */
interface OverviewStatDetail {
  readonly key: string;
  readonly label: string;
  readonly value: string;
  readonly change: number;
}

const noDetails: readonly OverviewStatDetail[] = [];

/**
 * Props for the OverviewStat component.
 */
interface OverviewStatProps {
  readonly label: string;
  readonly value: string;
  readonly caption: ReactNode;
  readonly details?: readonly OverviewStatDetail[];
  readonly detailsLabel?: string;
}

/**
 * Displays one headline number with a short label and caption, and an optional collapsible breakdown.
 */
const OverviewStat = function ({
  label,
  value,
  caption,
  details = noDetails,
  detailsLabel = "breakdown",
}: OverviewStatProps): JSX.Element {
  const [expanded, setExpanded] = useState(false);
  return (
    <Paper
      sx={{
        // Keeps a card its own height when a neighbor's breakdown is expanded
        alignSelf: "start",
        border: "1px solid",
        borderColor: "divider",
        borderRadius: 3,
        p: 2,
        display: "flex",
        flexDirection: "column",
        gap: 0.5,
      }}
    >
      <Stack direction="row" spacing={1} sx={{ alignItems: "center" }}>
        <Stack spacing={0.5} sx={{ flex: 1, minWidth: 0 }}>
          <Typography variant="body2" color="text.secondary">
            {label}
          </Typography>
          <Typography variant="h5" component="p" sx={{ fontWeight: 500 }}>
            {value}
          </Typography>
          <Typography variant="body2" color="text.secondary">
            {caption}
          </Typography>
        </Stack>
        {details.length === 0 ? null : (
          <IconButton
            size="small"
            aria-expanded={expanded}
            aria-label={`${expanded ? "Hide" : "Show"} ${label.toLowerCase()} ${detailsLabel}`}
            onClick={() => {
              setExpanded((current) => !current);
            }}
            sx={{ mr: -0.75 }}
          >
            <ExpandMore
              sx={{
                transform: expanded ? "rotate(180deg)" : "none",
                transition: "transform 0.2s ease-in-out",
              }}
            />
          </IconButton>
        )}
      </Stack>
      {details.length === 0 ? null : (
        <Collapse in={expanded} timeout="auto" unmountOnExit>
          <Stack spacing={1} sx={{ pt: 0.5 }}>
            {details.map((detail) => (
              <Stack
                key={detail.key}
                direction="row"
                spacing={1.5}
                sx={{
                  justifyContent: "space-between",
                  alignItems: "baseline",
                }}
              >
                <Typography variant="body2">{detail.label}</Typography>
                <Stack sx={{ alignItems: "flex-end" }}>
                  <Typography variant="body2" sx={{ fontWeight: 500 }}>
                    {detail.value}
                  </Typography>
                  <Typography variant="caption">
                    <OverviewChange amount={detail.change} />
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

export default OverviewStat;
export type { OverviewStatDetail };
