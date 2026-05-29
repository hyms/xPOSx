import { computed } from "vue";
import { useSettings } from "@/services/settings.service";

export function useCurrency() {
  const { settings } = useSettings();

  const currencySymbol = computed(() => settings.value?.currencySymbol || "Bs");
  const currencyCode = computed(() => {
    const code = settings.value?.currencyCode || "BOB";
    return (code === "Bs" || code === "Bs.") ? "BOB" : code;
  });

  const formatCurrency = (value: number | undefined | null): string => {
    if (value === null || typeof value === "undefined") {
      return `${currencySymbol.value} 0.00`;
    }

    // Use Intl.NumberFormat for robust, locale-aware currency formatting
    try {
      return new Intl.NumberFormat(undefined, {
        style: "currency",
        currency: currencyCode.value,
        currencyDisplay: "symbol",
      }).format(value);
    } catch (error) {
      // Fallback for invalid currency codes
      return `${currencySymbol.value} ${value.toFixed(2)}`;
    }
  };

  return {
    formatCurrency,
    currencySymbol,
    currencyCode,
  };
}
