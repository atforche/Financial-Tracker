import DesktopNavigation from "@navigation/DesktopNavigation";
import MobileNavigation from "@navigation/MobileNavigation";
import type { NavigationPage } from "@navigation/NavigationPage";
import useMobile from "@framework/useMobile";

/**
 * Props for the Navigation component.
 * @param {NavigationPage} initialPage - Initial page to be selected.
 * @param {Function} onNavigation - Callback to be executed whenever the navigation selection changes.
 */
interface NavigationProps {
  initialPage: NavigationPage;
  onNavigation: (val: NavigationPage) => void;
}

/**
 * Component that presents the user with a navigation layout.
 * @param {NavigationProps} props - Props for the Navigation component.
 * @returns {JSX.Element} JSX element representing the Navigation component.
 */
const Navigation = function ({
  initialPage,
  onNavigation,
}: NavigationProps): JSX.Element {
  const isMobile = useMobile();

  if (isMobile) {
    return (
      <MobileNavigation initialPage={initialPage} onNavigation={onNavigation} />
    );
  }
  return (
    <DesktopNavigation initialPage={initialPage} onNavigation={onNavigation} />
  );
};

export default Navigation;
