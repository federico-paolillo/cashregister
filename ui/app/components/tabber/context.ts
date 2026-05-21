import { createContext, useContext } from "react";

export interface TabberContextValue {
  activeTab: string;
  baseId: string;
  setActiveTab: (tabId: string) => void;
}

export const TabberContext = createContext<TabberContextValue | undefined>(
  undefined,
);

export function useTabberContext(): TabberContextValue {
  const context = useContext(TabberContext);

  if (context === undefined) {
    throw new Error("Tabber components must be used within a <Tabber>");
  }

  return context;
}

export function getTabElementId(baseId: string, tabId: string) {
  return `${baseId}-tab-${encodeURIComponent(tabId)}`;
}

export function getTabPanelElementId(baseId: string, tabId: string) {
  return `${baseId}-panel-${encodeURIComponent(tabId)}`;
}
