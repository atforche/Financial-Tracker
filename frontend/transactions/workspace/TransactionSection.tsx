"use client";

import { Box, Chip, Paper, Stack, Typography, alpha } from "@mui/material";
import type { JSX, ReactNode } from "react";

/**
 * Props for the TransactionSection component.
 */
interface TransactionSectionProps {
  readonly title: string;
  readonly children: ReactNode;
  readonly headerContent?: ReactNode;

  /**
   * Whether the section still needs input before the transaction can be saved.
   */
  readonly incomplete?: boolean;
}

/**
 * A section of a transaction page under a tinted header with its title, an Incomplete tag while it still needs input,
 * and optional header actions.
 */
const TransactionSection = function ({
  title,
  children,
  headerContent = null,
  incomplete = false,
}: TransactionSectionProps): JSX.Element {
  const color = incomplete ? "warning" : "info";

  return (
    <Paper
      variant="outlined"
      sx={(theme) => ({
        borderRadius: 3,
        overflow: "hidden",
        borderColor: alpha(theme.palette[color].main, 0.35),
      })}
    >
      <Stack
        direction="row"
        spacing={1.25}
        useFlexGap
        sx={(theme) => ({
          alignItems: "center",
          flexWrap: "wrap",
          minHeight: 32,
          px: { xs: 1.5, sm: 2 },
          py: 1.25,
          backgroundColor: alpha(theme.palette[color].main, 0.08),
          borderBottom: `1px solid ${alpha(theme.palette[color].main, 0.16)}`,
        })}
      >
        <Box
          sx={(theme) => ({
            width: 10,
            height: 10,
            borderRadius: "50%",
            flexShrink: 0,
            backgroundColor: theme.palette[color].main,
            boxShadow: `0 0 0 4px ${alpha(theme.palette[color].main, 0.16)}`,
          })}
        />
        <Typography variant="h6" component="h2" sx={{ flexGrow: 1 }}>
          {title}
        </Typography>
        {incomplete ? (
          <Chip
            label="Incomplete"
            size="small"
            color="warning"
            variant="outlined"
          />
        ) : null}
        {headerContent}
      </Stack>
      <Stack spacing={2} sx={{ p: { xs: 1.5, sm: 2 } }}>
        {children}
      </Stack>
    </Paper>
  );
};

export default TransactionSection;
