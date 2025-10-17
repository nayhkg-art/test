// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("4w4aBNokh+73BPVQd2i9XTdB07/lZmhnV+VmbWXlZmZnrJfYFp9j4xuEY5ZXa1b8WozMXKxbvFdekl77V+VmRVdqYW5N4S/hkGpmZmZiZ2TZ2BCm0uiu/KjWN+s+ZIm2P3aWpzpcvyfl55Dy8ScPcnhDzw+eHLKiB54c+jBzJ24O5+hrmpUz8FPuHakM3yl+NipaG3OjBf1cOcze+lJEi7FXxZ/BM7gx/vTi9ZHVcuQaoNvKHuPwFlWGD3ptok74arv513mrbYFX5fsoExb+SC6Iq/vA6zLWxb21qnaLbPMn1onOUGzIRG6NhloS2ag6lQHJVaQPKHhhKcqJZgkajZsrwwW6LxuaN9lLsE3alm30Wcf/yP9Hdm9yJpV1zbO3mmVkZmdm");
        private static int[] order = new int[] { 13,1,7,13,4,8,7,8,13,12,13,13,12,13,14 };
        private static int key = 103;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
