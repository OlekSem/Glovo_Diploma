export type FormFeedback = { message: string; fields: Record<string, string[]> };

const toFieldName = (name: string) => {
    const leaf = name.split(".").at(-1) ?? name;
    return leaf ? leaf.charAt(0).toLowerCase() + leaf.slice(1) : name;
};

const messagesFrom = (value: unknown): string[] => {
    if (typeof value === "string") return [value];
    if (Array.isArray(value)) return value.flatMap(messagesFrom);
    if (typeof value === "object" && value !== null) {
        const item = value as Record<string, unknown>;
        if (typeof item.description === "string") return [item.description];
        if (typeof item.message === "string") return [item.message];
        if (typeof item.title === "string") return [item.title];
    }
    return [];
};

const fieldForIdentityError = (value: unknown) => {
    if (typeof value !== "object" || value === null) return undefined;
    const code = (value as { code?: unknown }).code;
    return typeof code === "string" && code.startsWith("Password") ? "password" : undefined;
};

export const parseApiFailure = (error: unknown): FormFeedback => {
    const fallback = "The request could not be completed.";
    const response = typeof error === "object" && error !== null && "data" in error
        ? (error as { data?: unknown }).data
        : undefined;
    if (typeof response === "string") return { message: response, fields: {} };

    const fields: Record<string, string[]> = {};
    const addField = (field: string, messages: string[]) => {
        if (messages.length > 0) fields[toFieldName(field)] = messages;
    };

    if (Array.isArray(response)) {
        response.forEach((item) => {
            const messages = messagesFrom(item);
            const field = fieldForIdentityError(item);
            if (field) addField(field, messages);
        });
        const messages = response.flatMap(messagesFrom);
        return { message: messages.join(" ") || fallback, fields };
    }

    if (typeof response === "object" && response !== null) {
        const body = response as Record<string, unknown>;
        if (typeof body.errors === "object" && body.errors !== null && !Array.isArray(body.errors)) {
            Object.entries(body.errors as Record<string, unknown>).forEach(([field, value]) => addField(field, messagesFrom(value)));
        } else if (Array.isArray(body.errors)) {
            body.errors.forEach((item) => {
                const messages = messagesFrom(item);
                const field = fieldForIdentityError(item);
                if (field) addField(field, messages);
            });
        }
        const message = typeof body.message === "string" ? body.message
            : typeof body.title === "string" ? body.title
                : typeof body.description === "string" ? body.description
                    : Object.keys(fields).length > 0 ? "Please correct the highlighted fields." : fallback;
        return { message, fields };
    }

    if (typeof error === "object" && error !== null && "error" in error) {
        const message = (error as { error?: unknown }).error;
        if (typeof message === "string") return { message, fields: {} };
    }
    return { message: fallback, fields: {} };
};
