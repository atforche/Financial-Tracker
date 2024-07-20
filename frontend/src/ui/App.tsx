import { Box, useMediaQuery, useTheme } from "@mui/material";
import NavigationPage from "./NavigationPage";
import PermanentNavigation from "./PermanentNavigation";
import TemporaryNavigation from "./TemporaryNavigation";
import { useState } from "react";

/**
 * Primary component that manages the application.
 * Provides the navigation layout and manages which content components are displayed.
 * @returns {JSX.Element} JSX element representing the entire application.
 */
const App = function (): JSX.Element {
  const [currentPage, setCurrentPage] = useState(NavigationPage.Overview);

  // Use the permanent navigation variant for any screen sizes "md" or larger.
  // Use the temporary navigation variant for any screen sizes smaller than "md".
  const theme = useTheme();
  const useTemporary = useMediaQuery(theme.breakpoints.down("md"));

  return (
    <Box>
      {useTemporary ? (
        <TemporaryNavigation
          initialPage={currentPage}
          onNavigation={setCurrentPage}
        />
      ) : (
        <PermanentNavigation
          initialPage={currentPage}
          onNavigation={setCurrentPage}
        />
      )}
    </Box>
  );
};

export default App;
