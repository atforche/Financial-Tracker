"use client";

import {
  Accordion,
  AccordionDetails,
  AccordionSummary,
  Typography,
} from "@mui/material";
import type { JSX, ReactNode } from "react";
import ExpandMore from "@mui/icons-material/ExpandMore";
import { useSearchParams } from "next/navigation";

/**
 * Search parameter holding the expanded financial institution.
 */
const institutionParamName = "institution";

/**
 * Props for the InstitutionSection component.
 */
interface InstitutionSectionProps {
  readonly institution: string;
  readonly accountCount: number;
  readonly children: ReactNode;
}

/**
 * A full-width, collapsed-by-default section for one financial institution. The expanded institution is kept in the
 * URL so returning from an account keeps the user's place.
 */
const InstitutionSection = function ({
  institution,
  accountCount,
  children,
}: InstitutionSectionProps): JSX.Element {
  const searchParams = useSearchParams();
  const isExpanded = searchParams.get(institutionParamName) === institution;

  return (
    <Accordion
      expanded={isExpanded}
      onChange={(_, expanded) => {
        // Updates the URL without a server round trip; Next keeps useSearchParams in sync
        const params = new URLSearchParams(searchParams.toString());
        if (expanded) {
          params.set(institutionParamName, institution);
        } else {
          params.delete(institutionParamName);
        }
        const query = params.toString();
        window.history.replaceState(
          null,
          "",
          query === "" ? window.location.pathname : `?${query}`,
        );
      }}
      disableGutters
      variant="outlined"
      slotProps={{ transition: { unmountOnExit: true } }}
      sx={{ borderRadius: 3, "&::before": { display: "none" } }}
    >
      <AccordionSummary expandIcon={<ExpandMore />}>
        <Typography variant="subtitle1" component="h2" sx={{ flexGrow: 1 }}>
          {institution}
        </Typography>
        <Typography
          variant="body2"
          color="text.secondary"
          sx={{ alignSelf: "center", mr: 1 }}
        >
          {accountCount === 1 ? "1 account" : `${accountCount} accounts`}
        </Typography>
      </AccordionSummary>
      <AccordionDetails sx={{ pt: 0 }}>{children}</AccordionDetails>
    </Accordion>
  );
};

export default InstitutionSection;
