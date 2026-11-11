"use client";

import { Box, Paper, Stack, Typography } from "@mui/material";
import type { JSX } from "react";
import KeyboardArrowRight from "@mui/icons-material/KeyboardArrowRight";
import Link from "next/link";
import type { Route } from "next";

/**
 * Props for the EntityCard component.
 */
interface EntityCardProps {
  readonly name: string;
  readonly href: Route;
  readonly color: "primary" | "info" | "warning" | "error";
  readonly balance: string;
}

/**
 * Shows an account or fund's current balance, linking to it.
 */
const EntityCard = function ({
  name,
  href,
  color,
  balance,
}: EntityCardProps): JSX.Element {
  return (
    <Paper
      component={Link}
      href={href}
      sx={{
        display: "grid",
        alignItems: "center",
        // Phones show one compact row; wider screens put the balance under the name
        gridTemplateAreas: {
          xs: '"name balance chevron"',
          sm: '"name chevron" "balance balance"',
        },
        gridTemplateColumns: { xs: "minmax(0, 1fr) auto auto", sm: "1fr auto" },
        columnGap: 1,
        rowGap: 1.5,
        p: { xs: 1.5, sm: 2 },
        border: "1px solid",
        borderColor: "divider",
        borderRadius: 3,
        color: "inherit",
        textDecoration: "none",
        "&:hover": { borderColor: "primary.main" },
      }}
    >
      <Stack
        direction="row"
        spacing={1}
        sx={{ gridArea: "name", alignItems: "center", minWidth: 0 }}
      >
        <Box
          sx={{
            width: 10,
            height: 10,
            flexShrink: 0,
            borderRadius: "50%",
            bgcolor: `${color}.main`,
          }}
        />
        <Typography variant="subtitle1" noWrap sx={{ fontWeight: 600 }}>
          {name}
        </Typography>
      </Stack>
      <KeyboardArrowRight
        sx={{ gridArea: "chevron", color: "text.secondary", fontSize: 22 }}
      />
      <Stack spacing={0.25} sx={{ gridArea: "balance" }}>
        <Typography
          variant="body2"
          color="text.secondary"
          sx={{ display: { xs: "none", sm: "block" } }}
        >
          Current balance
        </Typography>
        <Typography
          variant="h5"
          component="p"
          sx={{
            fontWeight: 500,
            fontSize: { xs: "1rem", sm: "1.5rem" },
            whiteSpace: "nowrap",
          }}
        >
          {balance}
        </Typography>
      </Stack>
    </Paper>
  );
};

export default EntityCard;
