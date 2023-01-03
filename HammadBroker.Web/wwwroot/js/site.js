function previewImage(input, imgElement, placeholder) {
	if (typeof(FileReader) === "undefined") {
		console.error("This browser does not support HTML FileReader.");
		return;
	}

	if (!imgElement) return;

	if (!input || !input.files || !input.files.length) {
		imgElement.src = placeholder;
		return;
	}

	const file = input.files[0];
	const reader = new FileReader();
	reader.onload = e => {
		imgElement.src = e.target.result;
	}
	reader.readAsDataURL(file);
}

function toUrlParams(params, prefix = "") {
	if (!params) return null;
	return Object.entries(params)
		.filter(([_, val]) => val !== null && val !== undefined)
		.map(([key, val]) => {
			if (val === null || val === undefined) return null;
			return (val instanceof Object)
				? toUrlParams(val, `${prefix}${key}.`)
				: `${prefix}${key}=${encodeURIComponent(val)}`;
		})
		.filter(e => e !== null && e !== undefined)
		.join("&");
}

function parseFunction(str) {
	if (!str) return null;
	str = str.trim();
	if (!str.length) return null;

	const is_async = str.startsWith("async"),
		ndx = str.indexOf("{"),
		fn_body = str.substring(ndx + 1, str.lastIndexOf("}")),
		fn_declare = str.substring(0, ndx),
		fn_params = fn_declare.substring(fn_declare.indexOf("(")+1, fn_declare.lastIndexOf(")")),
		args = fn_params.split(",");
	args.push(fn_body);

	if (is_async) {
		const AsyncFunction = Object.getPrototypeOf(async function() {}).constructor;

		function fn() {
			return AsyncFunction.apply(this, args);
		}
		fn.prototype = AsyncFunction.prototype;
	} else {
		function fn() {
			return Function.apply(this, args);
		}
		fn.prototype = Function.prototype;
	}

	return new fn();
}

function rgxFormat(str) {
	for (let i = arguments.length - 1; i > 0; i--) {
		str = str.replace(new RegExp(`\\{${(i - 1)}\\}`, "gm"), (arguments[i] || "").toString());
	}

	return str;
}

function *range(start, end, step = 1) {
	for (let i = start; i <= end; i += step) {
		yield i;
	}
}

function getTotalPages(pageSize, totalCount) {
	return ~~Math.ceil(totalCount / pageSize.toFixed(2));
}

function isActivePage(currentPage, currentIteration) {
	return currentPage === currentIteration;
}
		
function getMaxPage(maxPages, currentPage) {
	return ~~Math.ceil(currentPage / maxPages.toFixed(2)) * maxPages;
}
		
function getMinPageToRender(maxPages, totalPages, currentPage) {
	let minPageToRender = getMaxPageToRender(maxPages, totalPages, currentPage);
	if (minPageToRender === 1) return minPageToRender;
	if (minPageToRender === totalPages) minPageToRender = getMaxPage(maxPages, currentPage);
	return minPageToRender - maxPages + 1;
}

function getMaxPageToRender(maxPages, totalPages, currentPage) {
	const maxPage = getMaxPage(maxPages, currentPage);
	return maxPage <= totalPages ? maxPage : totalPages;
}

function showLeftPagerButton(maxPages, totalPages, currentPage) {
	return getMinPageToRender(maxPages, totalPages, currentPage) > maxPages;
}

function showRightPagerButton(maxPages, totalPages, currentPage) {
	return getMaxPageToRender(maxPages, totalPages, currentPage) < totalPages;
}
