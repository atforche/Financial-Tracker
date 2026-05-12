import { Box, Stack } from "@mui/material";
import type { JSX } from "react";
import Navigation from "@/app/Navigation";
import getOnboardingEligibility from "@/onboarding/getOnboardingEligibility";
import { redirect } from "next/navigation";

/**
 * Layout that protects the main application until onboarding is complete.
 */
const MainAppLayout = async function ({
  children,
}: Readonly<{
  children: JSX.Element;
}>): Promise<JSX.Element> {
  const eligibility = await getOnboardingEligibility();
  if (eligibility.isEligible) {
    redirect("/onboarding");
  }

  return (
    <Stack direction="row">
      <Navigation />
      <Box sx={{ padding: "25px", width: "100%" }}>{children}</Box>
    </Stack>
  );
};

export const dynamic = "force-dynamic";
export default MainAppLayout;
