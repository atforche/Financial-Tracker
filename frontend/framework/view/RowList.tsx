"use client";

import {
  Box,
  Button,
  ButtonBase,
  Chip,
  Stack,
  Typography,
} from "@mui/material";
import { type JSX, useState } from "react";
import ChevronRight from "@mui/icons-material/ChevronRight";
import Link from "next/link";
import type { Route } from "next";

/**
 * One row of a RowList.
 */
interface RowListItem {
  readonly key: string;
  readonly href: Route;
  readonly leading: string;
  readonly title: string;
  readonly detail: string;
  readonly value: string;
  readonly valueColor?: string;
  readonly tag?: string;
}

/**
 * Props for the RowList component.
 */
interface RowListProps {
  readonly rows: readonly RowListItem[];
  readonly initialRowCount?: number;
}

/**
 * Lists linked items as compact rows on a surface, one line each on wider screens and two on a phone. A long list
 * starts collapsed and expands in place.
 */
const RowList = function ({
  rows,
  initialRowCount = 10,
}: RowListProps): JSX.Element {
  const [showAll, setShowAll] = useState(false);
  const visibleRows = showAll ? rows : rows.slice(0, initialRowCount);
  return (
    <Stack>
      {visibleRows.map((row, index) => (
        <ButtonBase
          key={row.key}
          component={Link}
          href={row.href}
          sx={{
            display: "grid",
            alignItems: "center",
            columnGap: 2,
            rowGap: 0.25,
            gridTemplateColumns: {
              xs: "6rem minmax(0, 1fr) auto auto",
              md: "6rem minmax(0, 2fr) minmax(0, 1.5fr) 7.5rem auto",
            },
            gridTemplateAreas: {
              xs: `"title title value chevron" "leading detail detail chevron"`,
              md: `"leading title detail value chevron"`,
            },
            px: 1,
            py: 1,
            borderTop: index === 0 ? "none" : "1px solid",
            borderColor: "divider",
            textAlign: "left",
            "&:hover": { bgcolor: "action.hover" },
          }}
        >
          <Typography
            variant="body2"
            color="text.secondary"
            sx={{ gridArea: "leading", whiteSpace: "nowrap" }}
          >
            {row.leading}
          </Typography>
          <Stack
            direction="row"
            spacing={1}
            sx={{ gridArea: "title", alignItems: "center", minWidth: 0 }}
          >
            <Typography variant="body2" noWrap sx={{ fontWeight: 500 }}>
              {row.title}
            </Typography>
            {row.tag === undefined ? null : (
              <Chip label={row.tag} size="small" variant="outlined" />
            )}
          </Stack>
          <Typography
            variant="body2"
            color="text.secondary"
            noWrap
            sx={{ gridArea: "detail" }}
          >
            {row.detail}
          </Typography>
          <Typography
            variant="body2"
            sx={{
              gridArea: "value",
              color: row.valueColor,
              fontWeight: 600,
              textAlign: "right",
              whiteSpace: "nowrap",
            }}
          >
            {row.value}
          </Typography>
          <Box sx={{ gridArea: "chevron", display: "flex" }}>
            <ChevronRight fontSize="small" color="action" />
          </Box>
        </ButtonBase>
      ))}
      {!showAll && rows.length > initialRowCount ? (
        <Button
          size="small"
          onClick={() => {
            setShowAll(true);
          }}
          sx={{ alignSelf: "flex-end", mt: 1 }}
        >
          Show all {rows.length}
        </Button>
      ) : null}
    </Stack>
  );
};

export default RowList;
