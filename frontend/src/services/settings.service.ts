import { ref, readonly, computed } from 'vue';
import api from '@/api';
import type { Setting } from '@/types/settings';
import type { CurrencySetting } from '@/types';

// Private state
const settingsData = ref<Setting | null>(null);
const currencySettings = ref<CurrencySetting | null>(null);
const isLoaded = ref(false);
const isLoading = ref(false);

interface SettingsAsObject {
  [key: string]: string;
}

// Computed property to transform the object into a more convenient key-value object
const settings = computed<SettingsAsObject>(() => {
  const allSettings: SettingsAsObject = {};
  if (settingsData.value) {
    for (const key in settingsData.value) {
      if (Object.prototype.hasOwnProperty.call(settingsData.value, key)) {
        // Ensure the value is converted to a string if needed for SettingsAsObject type
        allSettings[key] = String((settingsData.value as any)[key]);
      }
    }
  }
  
  if(currencySettings.value) {
    allSettings.currencySymbol = currencySettings.value.symbol;
    allSettings.currencyCode = currencySettings.value.code;
  }
  
  return allSettings;
});

// --- Private methods ---

async function fetchSettings() {
  if (isLoaded.value || isLoading.value) {
    return;
  }
  isLoading.value = true;
  try {
    const settingsResponse = await api.get<any>('/settings');
    settingsData.value = settingsResponse.data;
    if (settingsResponse.data) {
      currencySettings.value = {
        id: settingsResponse.data.id,
        code: settingsResponse.data.currencyCode || 'BOB',
        symbol: settingsResponse.data.currencySymbol || 'Bs'
      };
    }
    isLoaded.value = true;
  } catch (error) {
    console.error("Error fetching settings:", error);
  } finally {
    isLoading.value = false;
  }
}

async function updateSingleSetting(key: string, value: string) {
    if (!settingsData.value) {
        console.error(`Settings data not loaded.`);
        return;
    }

    const originalValue = (settingsData.value as any)[key];
    (settingsData.value as any)[key] = value;

    try {
        await api.put(`/settings/${settingsData.value.id}`, settingsData.value);
    } catch (error) {
        (settingsData.value as any)[key] = originalValue;
        console.error(`Failed to update setting ${key}:`, error);
        throw error;
    }
}

// --- Public Singleton ---

// This is the composable that will be used by the components
export function useSettings() {
  return {
    settings,
    isLoaded: readonly(isLoaded),
    isLoading: readonly(isLoading),
    fetchSettings,
    updateSingleSetting,
  };
}
