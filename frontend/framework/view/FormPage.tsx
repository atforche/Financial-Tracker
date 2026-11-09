"use client";

import { Box, Button, Stack, Typography } from "@mui/material";
import type { JSX, ReactNode } from "react";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import Link from "next/link";
import PageLayout from "@/framework/view/PageLayout";
import ReturnLink from "@/framework/view/ReturnLink";
import type { Route } from "next";

/**
 * Props for the FormPage component.
 */
interface FormPageProps {
  readonly title: string;
  readonly returnHref: Route;
  readonly returnLabel: string;
  readonly submitLabel: string;
  readonly canSubmit: boolean;
  readonly pending: boolean;
  readonly onSubmit: () => void;
  readonly errorTitle: string | null;
  readonly unmappedErrors: string | null;

  /**
   * The form's sections, stacked in one column.
   */
  readonly children: ReactNode;
}

/**
 * Lays out a full-page form: a return link, the title with Cancel and submit actions, any errors, and the form's
 * sections. On a phone the actions stay pinned to the bottom of the screen.
 */
const FormPage = function ({
  title,
  returnHref,
  returnLabel,
  submitLabel,
  canSubmit,
  pending,
  onSubmit,
  errorTitle,
  unmappedErrors,
  children,
}: FormPageProps): JSX.Element {
  const actions = (
    <Stack
      direction="row"
      spacing={1}
      sx={{ "& > *": { flex: { xs: 1, sm: "none" } } }}
    >
      <Button
        variant="outlined"
        disabled={pending}
        component={Link}
        href={returnHref}
      >
        Cancel
      </Button>
      <Button
        variant="contained"
        loading={pending}
        disabled={!canSubmit}
        onClick={onSubmit}
      >
        {submitLabel}
      </Button>
    </Stack>
  );

  return (
    <PageLayout>
      <Stack spacing={2} sx={{ width: "100%", maxWidth: 780, mx: "auto" }}>
        <ReturnLink
          returnUrl={undefined}
          fallback={{ href: returnHref, label: returnLabel }}
        />
        <Stack
          direction="row"
          useFlexGap
          spacing={2}
          sx={{
            flexWrap: "wrap",
            alignItems: "center",
            justifyContent: "space-between",
          }}
        >
          <Typography variant="h4">{title}</Typography>
          <Box sx={{ display: { xs: "none", sm: "block" } }}>{actions}</Box>
        </Stack>
        <ErrorAlert errorMessage={errorTitle} unmappedErrors={unmappedErrors} />
        {children}
      </Stack>
      {/* On small screens the actions stay pinned to the bottom so they remain in reach while scrolling */}
      <Box
        sx={{
          display: { xs: "block", sm: "none" },
          position: "sticky",
          bottom: 0,
          zIndex: "appBar",
          py: 1.5,
          bgcolor: "background.default",
          borderTop: 1,
          borderColor: "divider",
        }}
      >
        {actions}
      </Box>
    </PageLayout>
  );
};

export default FormPage;
