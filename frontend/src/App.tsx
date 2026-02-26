import { type JSX, useState } from "react";
import {
  NavigationContent,
  type NavigationPage,
} from "@navigation/NavigationPage";
import { Box } from "@mui/material";
import Navigation from "@navigation/Navigation";
import Stack from "@mui/material/Stack";
import useMobile from "@framework/useMobile";

/**
 * Primary component that manages the application.
 * Provides the navigation layout and manages which content components are displayed.
 * @returns JSX element representing the entire application.
 */
const App = function (): JSX.Element {
  const [currentPage, setCurrentPage] = useState<NavigationPage>("Overview");
  const isMobile = useMobile();

  return (
    <Stack direction={isMobile ? "column" : "row"}>
      <Navigation initialPage={currentPage} onNavigation={setCurrentPage} />
      <Box sx={{ padding: "25px", width: "100%" }}>
        {NavigationContent[currentPage]}
      </Box>
    </Stack>
  );
};

export default App;
