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
        display: "flex",
        flexDirection: "column",
        gap: 1.5,
        p: 2,
        border: "1px solid",
        borderColor: "divider",
        borderRadius: 3,
        color: "inherit",
        textDecoration: "none",
        "&:hover": { borderColor: "primary.main" },
      }}
    >
      <Stack direction="row" spacing={1} sx={{ alignItems: "center" }}>
        <Box
          sx={{
            width: 10,
            height: 10,
            flexShrink: 0,
            borderRadius: "50%",
            bgcolor: `${color}.main`,
          }}
        />
        <Typography variant="subtitle1" sx={{ flexGrow: 1, fontWeight: 600 }}>
          {name}
        </Typography>
        <KeyboardArrowRight sx={{ color: "text.secondary", fontSize: 22 }} />
      </Stack>
      <Stack spacing={0.25}>
        <Typography variant="body2" color="text.secondary">
          Current balance
        </Typography>
        <Typography variant="h5" component="p" sx={{ fontWeight: 500 }}>
          {balance}
        </Typography>
      </Stack>
    </Paper>
  );
};

export default EntityCard;
