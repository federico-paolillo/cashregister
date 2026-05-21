import type { ReactNode } from "react";
import {
  getTabElementId,
  getTabPanelElementId,
  useTabberContext,
} from "@cashregister/components/tabber/context";

interface TabPanelProps {
  tabId: string;
  children: ReactNode;
}

export function TabPanel({ tabId, children }: TabPanelProps) {
  const { activeTab, baseId } = useTabberContext();

  return (
    <section
      aria-labelledby={getTabElementId(baseId, tabId)}
      hidden={activeTab !== tabId}
      id={getTabPanelElementId(baseId, tabId)}
      role="tabpanel"
      tabIndex={0}
    >
      {children}
    </section>
  );
}
