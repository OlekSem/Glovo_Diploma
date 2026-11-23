import {useEffect, useState} from "react";

type Props = {
    id: string;
    label: string;
    value?: string | null;
    onChange: (file: File | null) => void;
    aspectClassName?: string;
};

export default function ImageFilePicker({id, label, value, onChange, aspectClassName = "aspect-[4/3]"}: Props) {
    const [preview, setPreview] = useState<string | null>(null);

    useEffect(() => () => { if (preview) URL.revokeObjectURL(preview); }, [preview]);

    const selectFile = (file: File | null) => {
        if (preview) URL.revokeObjectURL(preview);
        setPreview(file ? URL.createObjectURL(file) : null);
        onChange(file);
    };

    const image = preview ?? value;
    return <div className="mt-4">
        <p className="text-sm font-medium">{label}</p>
        <label htmlFor={id} className={`mt-1 flex ${aspectClassName} cursor-pointer items-center justify-center overflow-hidden rounded-2xl border-2 border-dashed border-slate-200 bg-slate-50 transition hover:border-amber-400 hover:bg-amber-50`}>
            {image
                ? <img src={image} alt={label} className="h-full w-full object-cover"/>
                : <span className="px-4 text-center text-sm text-slate-500"><span className="block text-2xl text-amber-500">▧</span>Додайте зображення<br/><small>PNG, JPG або WEBP · до 8 МБ</small></span>}
        </label>
        <input id={id} type="file" accept="image/png,image/jpeg,image/webp" className="sr-only" onChange={(event) => selectFile(event.target.files?.[0] ?? null)}/>
        {image && <div className="mt-2 flex gap-3 text-xs"><label htmlFor={id} className="cursor-pointer font-medium text-amber-700">Змінити</label><button type="button" className="text-slate-500 underline" onClick={() => selectFile(null)}>Скасувати вибір</button></div>}
    </div>;
}
