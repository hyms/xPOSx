import { ref, reactive, computed } from "vue";
import { useQuasar } from "quasar";
import { useRouter } from "vue-router";
import { productService } from "@/services/product.service";
import { categoryService } from "@/services/category.service";
import { warehouseService } from "@/services/warehouse.service";
import { clientService } from "@/services/client.service";
import { saleService } from "@/services/sale.service";
import { useCurrency } from "@/composables/useCurrency";
import type { Product, Category, Warehouse, Client, Sale, SaleDetail } from "@/types";

export function usePosLogic() {
    const $q = useQuasar();
    const router = useRouter();
    const { formatCurrency } = useCurrency();

    // State
    const search = ref("");
    const selectedCategory = ref(null);
    const categories = ref<Category[]>([]);
    const allProducts = ref<Product[]>([]);
    const warehouses = ref<Warehouse[]>([]);
    const clients = ref<Client[]>([]);
    const stocks = ref<Record<number, number>>({});

    const cart = ref<SaleDetail[]>([]);
    const showCheckoutDialog = ref(false);
    const showCartMobile = ref(false);
    const showScanner = ref(false);
    const showScanResultDialog = ref(false);
    const lastScannedProduct = ref<Product | null>(null);
    
    const paidAmount = ref(0);
    const change = ref(0);
    const saving = ref(false);

    const formData = reactive({
        clientId: 0,
        warehouseId: 0,
        date: new Date().toISOString().split("T")[0],
        taxRate: 0,
        discount: 0,
        shipping: 0,
        notes: "",
    });

    const quickAmounts = [10, 20, 50, 100];

    // Computed
    const subtotal = computed(() =>
        cart.value.reduce((sum: number, item: SaleDetail) => sum + item.total, 0),
    );

    const total = computed(() => {
        const taxAmount = subtotal.value * ((formData.taxRate || 0) / 100);
        return (
            subtotal.value +
            taxAmount +
            (formData.shipping || 0) -
            (formData.discount || 0)
        );
    });

    const filteredProducts = computed(() => {
        return allProducts.value.filter((p: Product) => {
            const matchesSearch =
                p.name.toLowerCase().includes(search.value.toLowerCase()) ||
                (p.code &&
                    p.code.toLowerCase().includes(search.value.toLowerCase()));
            const matchesCategory =
                !selectedCategory.value || p.categoryId === selectedCategory.value;
            return matchesSearch && matchesCategory;
        });
    });

    // Methods
    const calculateChange = () => {
        change.value = paidAmount.value - total.value;
    };

    const calculateTotals = () => {
        paidAmount.value = total.value;
        calculateChange();
    };

    const setQuickAmount = (amount: number) => {
        paidAmount.value = amount;
        calculateChange();
    };

    const addToCart = (product: Product) => {
        const existing = cart.value.find(
            (item: SaleDetail) => item.productId === product.id,
        );
        if (existing) {
            existing.quantity++;
            existing.total = existing.quantity * existing.price;
        } else {
            cart.value.push({
                productId: product.id!,
                quantity: 1,
                price: product.price,
                total: product.price,
            });
        }
        paidAmount.value = total.value;
        calculateChange();
    };

    const handleBarcodeScan = () => {
        const code = search.value.trim();
        if (!code) return;
        
        const exactMatch = allProducts.value.find((p) => 
            p.code?.trim() === code || 
            p.id?.toString() === code
        );

        if (exactMatch) {
            addToCart(exactMatch);
            search.value = "";
            lastScannedProduct.value = exactMatch;
            
            if (showScanner.value) {
                showScanResultDialog.value = true;
            } else {
                $q.notify({
                    color: "positive",
                    message: `<strong>${exactMatch.name}</strong> añadido al carrito`,
                    caption: `Precio: ${formatCurrency(exactMatch.price)}`,
                    icon: "shopping_cart",
                    html: true,
                    timeout: 1000,
                    position: 'center'
                });
            }
        } else {
            $q.notify({
                color: "warning",
                message: `Código "${code}" no encontrado`,
                icon: "error_outline",
                timeout: 2000,
            });
        }
    };

    const getProductName = (id: number) =>
        allProducts.value.find((p: Product) => p.id === id)?.name || "";

    const getStock = (productId: number) => stocks.value[productId] || 0;

    const fetchStocks = async () => {
        if (!formData.warehouseId) return;
        try {
            // Placeholder logic:
            allProducts.value.forEach((p: Product) => {
                stocks.value[p.id!] = Math.floor(Math.random() * 50) + 10; 
            });
        } catch (error) {}
    };

    const fetchData = async () => {
        try {
            const [pRes, cRes, wRes, clRes] = await Promise.all([
                productService.getAll(),
                categoryService.getAll(),
                warehouseService.getAll(),
                clientService.getAll(),
            ]);
            allProducts.value = pRes.data;
            categories.value = cRes.data;
            warehouses.value = wRes.data;
            clients.value = clRes.data;

            if (warehouses.value.length > 0) {
                formData.warehouseId = warehouses.value[0].id!;
                fetchStocks();
            }
            if (clients.value.length > 0) {
                formData.clientId = clients.value[0].id!;
            }
        } catch (error) {
            $q.notify({ color: "negative", message: "Error al cargar datos" });
        }
    };

    const incrementQty = (index: number) => {
        cart.value[index].quantity++;
        cart.value[index].total =
            cart.value[index].quantity * cart.value[index].price;
        paidAmount.value = total.value;
    };

    const decrementQty = (index: number) => {
        if (cart.value[index].quantity > 1) {
            cart.value[index].quantity--;
            cart.value[index].total =
                cart.value[index].quantity * cart.value[index].price;
            paidAmount.value = total.value;
        }
    };

    const removeFromCart = (index: number) => {
        cart.value.splice(index, 1);
        paidAmount.value = total.value;
    };

    const clearCart = () => {
        cart.value = [];
        paidAmount.value = 0;
        formData.taxRate = 0;
        formData.discount = 0;
        formData.shipping = 0;
        formData.notes = "";
    };

    const submitSale = async () => {
        saving.value = true;
        try {
            const sale: Sale = {
                ...formData,
                ref: `POS-${Date.now()}`,
                isPos: true,
                grandTotal: total.value,
                paidAmount: total.value,
                paymentStatus: "paid",
                status: "completed",
                details: cart.value,
            };

            const res = await saleService.create(sale);
            const saleId = res.data?.id || 1;

            clearCart();
            showCheckoutDialog.value = false;

            $q.dialog({
                title: "Venta Exitosa",
                message: `Venta realizada con éxito.\nTotal: ${formatCurrency(total.value)}\nCambio: ${formatCurrency(change.value)}`,
                persistent: true,
                ok: {
                    label: "Imprimir Comprobante",
                    color: "primary",
                    unelevated: true,
                },
                cancel: { label: "Cerrar", flat: true, color: "grey" },
            })
            .onOk(() => {
                router.push(`/sales/print/${saleId}`);
            });
        } catch (error) {
            $q.notify({ color: "negative", message: "Error al procesar la venta" });
        } finally {
            saving.value = false;
        }
    };

    const openCheckout = () => {
        showCartMobile.value = false;
        showCheckoutDialog.value = true;
    };

    return {
        // State
        search, selectedCategory, categories, allProducts, warehouses, clients, stocks,
        cart, showCheckoutDialog, showCartMobile, showScanner, showScanResultDialog,
        lastScannedProduct, paidAmount, change, saving, formData, quickAmounts,
        // Computed
        subtotal, total, filteredProducts,
        // Methods
        calculateTotals, setQuickAmount, handleBarcodeScan, getProductName, getStock,
        fetchData, fetchStocks, addToCart, incrementQty, decrementQty, removeFromCart,
        clearCart, calculateChange, submitSale, openCheckout
    };
}
