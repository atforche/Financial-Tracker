"use client";

import { Collapse, Divider, IconButton, Stack } from "@mui/material";
import { type JSX, type ReactNode, useId } from "react";
import ExpandMore from "@mui/icons-material/ExpandMore";
import SummaryCard from "@/framework/view/SummaryCard";

/**
 * Props for the ExpandableSummaryCard component.
 */
interface ExpandableSummaryCardProps {
  readonly title: string;
  readonly value: ReactNode;
  readonly expanded: boolean;
  readonly onToggle: () => void;
  readonly children: ReactNode;
}

/**
 * Displays a summary card with synchronized expandable content.
 */
const ExpandableSummaryCard = function ({
  title,
  value,
  expanded,
  onToggle,
  children,
}: ExpandableSummaryCardProps): JSX.Element {
  const detailsId = useId();

  return (
    <SummaryCard
      title={title}
      value={
        <Stack
          direction="row"
          alignItems="center"
          gap={0.5}
          justifyContent="space-between"
        >
          {value}
          <IconButton
            size="small"
            onClick={onToggle}
            aria-label={`${expanded ? "Collapse" : "Expand"} ${title}`}
            aria-expanded={expanded}
            aria-controls={detailsId}
            sx={{
              p: 0.25,
              transform: expanded ? "rotate(180deg)" : "rotate(0deg)",
              transition: "transform 0.3s ease-in-out",
            }}
          >
            <ExpandMore fontSize="small" />
          </IconButton>
        </Stack>
      }
    >
      <Collapse id={detailsId} in={expanded} timeout="auto" unmountOnExit>
        <Stack spacing={1.25} sx={{ pt: 1.25 }}>
          <Divider />
          {children}
        </Stack>
      </Collapse>
    </SummaryCard>
  );
};

export default ExpandableSummaryCard;
