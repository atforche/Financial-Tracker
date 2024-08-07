import { Stack, useMediaQuery, useTheme } from "@mui/material";
import AccountEntryList from "@ui/dataEntry/AccountEntryList";
import NavigationPage from "@core/fieldValues/NavigationPage";
import PermanentNavigation from "@ui/navigation/PermanentNavigation";
import TemporaryNavigation from "@ui/navigation/TemporaryNavigation";
import { useState } from "react";

/**
 * Primary component that manages the application.
 * Provides the navigation layout and manages which content components are displayed.
 * @returns {JSX.Element} JSX element representing the entire application.
 */
const App = function (): JSX.Element {
  const [currentPage, setCurrentPage] = useState<NavigationPage>(
    NavigationPage.Overview,
  );

  // Use the permanent navigation variant for any screen sizes "md" or larger.
  // Use the temporary navigation variant for any screen sizes smaller than "md".
  const theme = useTheme();
  const useTemporary = useMediaQuery(theme.breakpoints.down("md"));

  const buildTemporaryLayout = function (content: JSX.Element): JSX.Element {
    return (
      <Stack>
        <TemporaryNavigation
          initialPage={currentPage}
          onNavigation={setCurrentPage}
        />
        {content}
      </Stack>
    );
  };

  const buildPermanentLayout = function (content: JSX.Element): JSX.Element {
    return (
      <Stack direction="row">
        <PermanentNavigation
          initialPage={currentPage}
          onNavigation={setCurrentPage}
        />
        {content}
      </Stack>
    );
  };

  const buildContent = function (): JSX.Element {
    if (currentPage === NavigationPage.AccountEntry) {
      return <AccountEntryList />;
    }
    return <></>;
  };

  const content = buildContent();
  if (useTemporary) {
    return buildTemporaryLayout(content);
  }
  return buildPermanentLayout(content);
};

export default App;
